using System.Data;
using Dapper;

namespace StoreSyncBack.Services
{
    /// <summary>
    /// Implementação do serviço de migrations.
    /// Aplica scripts SQL versionados automaticamente na inicialização da aplicação.
    /// </summary>
    public class MigrationService : IMigrationService
    {
        private readonly IDbConnection _db;
        private readonly ILogger<MigrationService> _logger;
        private readonly string _migrationsPath;

        public MigrationService(
            IDbConnection db,
            ILogger<MigrationService> logger,
            IWebHostEnvironment environment)
        {
            _db = db;
            _logger = logger;
            _migrationsPath = Path.Combine(environment.ContentRootPath, "Migrations");
        }

        public async Task ApplyMigrationsAsync()
        {
            try
            {
                // Cria a tabela de controle de migrations se não existir
                await CreateVersionTableAsync();

                // Obtém o número da última migration aplicada
                var lastAppliedMigration = await GetLastAppliedMigrationAsync();
                _logger.LogInformation("Última migration aplicada: {LastMigration}",
                    lastAppliedMigration ?? "(nenhuma)");

                // Lista todas as migrations disponíveis no diretório
                var pendingMigrations = GetPendingMigrations(lastAppliedMigration);

                if (!pendingMigrations.Any())
                {
                    _logger.LogInformation("Nenhuma migration pendente para aplicar.");
                    return;
                }

                _logger.LogInformation("Encontradas {Count} migrations pendentes.", pendingMigrations.Count);

                // Aplica cada migration em ordem
                foreach (var migration in pendingMigrations)
                {
                    await ApplyMigrationAsync(migration);
                }

                _logger.LogInformation("Todas as migrations foram aplicadas com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao aplicar migrations.");
                throw;
            }
        }

        private async Task CreateVersionTableAsync()
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS historico_versao (
                    id SERIAL PRIMARY KEY,
                    numero_release VARCHAR(20) NOT NULL UNIQUE,
                    data_atualizacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
                );";

            await _db.ExecuteAsync(sql);
            _logger.LogDebug("Tabela historico_versao verificada/criada.");
        }

        private async Task<string?> GetLastAppliedMigrationAsync()
        {
            var sql = "SELECT numero_release FROM historico_versao ORDER BY numero_release DESC LIMIT 1;";
            return await _db.QueryFirstOrDefaultAsync<string?>(sql);
        }

        private List<MigrationInfo> GetPendingMigrations(string? lastAppliedMigration)
        {
            if (!Directory.Exists(_migrationsPath))
            {
                _logger.LogWarning("Diretório de migrations não encontrado: {Path}", _migrationsPath);
                return new List<MigrationInfo>();
            }

            var sqlFiles = Directory
                .GetFiles(_migrationsPath, "*.sql")
                .Select(Path.GetFileName)
                .Where(f => f != null)
                .Cast<string>()
                .OrderBy(f => f)
                .ToList();

            var pendingMigrations = new List<MigrationInfo>();

            foreach (var fileName in sqlFiles)
            {
                // Extrai o número da migration do nome do arquivo (ex: "001_migration.sql" -> "001")
                var migrationNumber = ExtractMigrationNumber(fileName);

                if (string.IsNullOrEmpty(migrationNumber))
                {
                    _logger.LogWarning("Arquivo de migration ignorado (formato inválido): {FileName}", fileName);
                    continue;
                }

                // Só inclui se for maior que a última migration aplicada
                if (string.IsNullOrEmpty(lastAppliedMigration) ||
                    string.Compare(migrationNumber, lastAppliedMigration, StringComparison.Ordinal) > 0)
                {
                    pendingMigrations.Add(new MigrationInfo
                    {
                        FileName = fileName,
                        MigrationNumber = migrationNumber,
                        FullPath = Path.Combine(_migrationsPath, fileName)
                    });
                }
            }

            return pendingMigrations;
        }

        private string? ExtractMigrationNumber(string fileName)
        {
            // Espera formato: XXX_descrição.sql (ex: 001_initial_schema.sql)
            var parts = fileName.Split('_');
            if (parts.Length >= 2)
            {
                return parts[0];
            }
            return null;
        }

        private async Task ApplyMigrationAsync(MigrationInfo migration)
        {
            _logger.LogInformation("Aplicando migration {MigrationNumber}: {FileName}...",
                migration.MigrationNumber, migration.FileName);

            var sql = await File.ReadAllTextAsync(migration.FullPath);

            using var transaction = _db.BeginTransaction();

            try
            {
                // Executa o script SQL da migration
                await _db.ExecuteAsync(sql, transaction: transaction);

                // Registra a migration na tabela de histórico
                await _db.ExecuteAsync(
                    "INSERT INTO historico_versao (numero_release) VALUES (@MigrationNumber);",
                    new { MigrationNumber = migration.MigrationNumber },
                    transaction);

                transaction.Commit();

                _logger.LogInformation("Migration {MigrationNumber} aplicada com sucesso.",
                    migration.MigrationNumber);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Erro ao aplicar migration {MigrationNumber}. Transaction foi revertida.",
                    migration.MigrationNumber);
                throw;
            }
        }

        private class MigrationInfo
        {
            public string FileName { get; set; } = string.Empty;
            public string MigrationNumber { get; set; } = string.Empty;
            public string FullPath { get; set; } = string.Empty;
        }
    }
}
