using System.Data;
using Dapper;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Repositories
{
    public class CaixaRepository : ICaixaRepository
    {
        private readonly IDbConnection _db;

        public CaixaRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<PaginatedResult<Caixa>> GetAllAsync(int limit, int offset)
        {
            var countSql = "SELECT COUNT(*) FROM caixa;";
            var totalCount = await _db.ExecuteScalarAsync<int>(countSql);

            var sql = @"
                SELECT
                    caixa_id         AS CaixaId,
                    referencia       AS Referencia,
                    valor_abertura   AS ValorAbertura,
                    valor_fechamento AS ValorFechamento,
                    total_vendas     AS TotalVendas,
                    total_sangrias   AS TotalSangrias,
                    total_suprimentos AS TotalSuprimentos,
                    valor_faltante   AS ValorFaltante,
                    valor_sobra      AS ValorSobra,
                    status           AS Status,
                    data_abertura    AS DataAbertura,
                    data_fechamento  AS DataFechamento
                FROM caixa
                ORDER BY data_abertura DESC
                LIMIT @Limit OFFSET @Offset;
            ";

            var items = await _db.QueryAsync<Caixa>(sql, new { Limit = limit, Offset = offset });

            return new PaginatedResult<Caixa>
            {
                Items = items,
                TotalCount = totalCount,
                Limit = limit,
                Offset = offset
            };
        }

        public async Task<Caixa?> GetByIdAsync(Guid id)
        {
            var sql = @"
                SELECT
                    caixa_id         AS CaixaId,
                    referencia       AS Referencia,
                    valor_abertura   AS ValorAbertura,
                    valor_fechamento AS ValorFechamento,
                    total_vendas     AS TotalVendas,
                    total_sangrias   AS TotalSangrias,
                    total_suprimentos AS TotalSuprimentos,
                    valor_faltante   AS ValorFaltante,
                    valor_sobra      AS ValorSobra,
                    status           AS Status,
                    data_abertura    AS DataAbertura,
                    data_fechamento  AS DataFechamento
                FROM caixa
                WHERE caixa_id = @Id;
            ";

            var caixa = await _db.QueryFirstOrDefaultAsync<Caixa>(sql, new { Id = id });
            if (caixa == null) return null;

            caixa.Vendas = (await GetVendasByCaixaAsync(id)).ToList();
            caixa.Movimentacoes = (await GetMovimentacoesByCaixaAsync(id)).ToList();

            return caixa;
        }

        public async Task<Caixa?> GetCaixaAbertoAsync()
        {
            var sql = @"
                SELECT
                    caixa_id         AS CaixaId,
                    referencia       AS Referencia,
                    valor_abertura   AS ValorAbertura,
                    valor_fechamento AS ValorFechamento,
                    total_vendas     AS TotalVendas,
                    total_sangrias   AS TotalSangrias,
                    total_suprimentos AS TotalSuprimentos,
                    valor_faltante   AS ValorFaltante,
                    valor_sobra      AS ValorSobra,
                    status           AS Status,
                    data_abertura    AS DataAbertura,
                    data_fechamento  AS DataFechamento
                FROM caixa
                WHERE status = @Status
                LIMIT 1;
            ";

            return await _db.QueryFirstOrDefaultAsync<Caixa>(sql, new { Status = CaixaStatus.Aberto });
        }

        public async Task<Guid> CreateAsync(Caixa caixa)
        {
            if (caixa.CaixaId == Guid.Empty)
                caixa.CaixaId = Guid.NewGuid();

            if (caixa.DataAbertura == default)
                caixa.DataAbertura = BrazilDateTime.Now;

            var sql = @"
                INSERT INTO caixa (caixa_id, referencia, valor_abertura, status, data_abertura)
                VALUES (@CaixaId, @Referencia, @ValorAbertura, @Status, @DataAbertura);
            ";

            await _db.ExecuteAsync(sql, caixa);
            return caixa.CaixaId;
        }

        public async Task<int> FecharAsync(Guid id, decimal valorFechamento, decimal totalVendas, decimal totalSangrias, decimal totalSuprimentos, decimal? valorFaltante, decimal? valorSobra)
        {
            var sql = @"
                UPDATE caixa SET
                    status            = @Status,
                    valor_fechamento  = @ValorFechamento,
                    total_vendas      = @TotalVendas,
                    total_sangrias    = @TotalSangrias,
                    total_suprimentos = @TotalSuprimentos,
                    valor_faltante    = @ValorFaltante,
                    valor_sobra       = @ValorSobra,
                    data_fechamento   = @DataFechamento
                WHERE caixa_id = @Id AND status = @StatusAberto;
            ";

            return await _db.ExecuteAsync(sql, new
            {
                Id = id,
                Status = CaixaStatus.Fechado,
                ValorFechamento = valorFechamento,
                TotalVendas = totalVendas,
                TotalSangrias = totalSangrias,
                TotalSuprimentos = totalSuprimentos,
                ValorFaltante = valorFaltante,
                ValorSobra = valorSobra,
                DataFechamento = BrazilDateTime.Now,
                StatusAberto = CaixaStatus.Aberto
            });
        }

        public async Task<int> AddMovimentacaoAsync(MovimentacaoCaixa mov)
        {
            if (mov.MovimentacaoCaixaId == Guid.Empty)
                mov.MovimentacaoCaixaId = Guid.NewGuid();

            if (mov.CreatedAt == default)
                mov.CreatedAt = BrazilDateTime.Now;

            var sql = @"
                INSERT INTO movimentacao_caixa (movimentacao_caixa_id, caixa_id, tipo, descricao, valor, created_at)
                VALUES (@MovimentacaoCaixaId, @CaixaId, @Tipo, @Descricao, @Valor, @CreatedAt);
            ";

            return await _db.ExecuteAsync(sql, mov);
        }

        public async Task<IEnumerable<Sale>> GetVendasByCaixaAsync(Guid caixaId)
        {
            var sql = @"
                SELECT
                    s.sale_id      AS SaleId,
                    s.referencia   AS Referencia,
                    s.total_amount AS TotalAmount,
                    s.status       AS Status,
                    s.sale_date    AS SaleDate,
                    e.employee_id  AS EmployeeId,
                    e.name         AS Name
                FROM sale s
                LEFT JOIN employee e ON e.employee_id = s.employee_id
                WHERE s.caixa_id = @CaixaId
                ORDER BY s.sale_date ASC;
            ";

            return await _db.QueryAsync<Sale, Employee, Sale>(
                sql,
                (sale, employee) =>
                {
                    sale.Employee = employee;
                    return sale;
                },
                new { CaixaId = caixaId },
                splitOn: "EmployeeId"
            );
        }

        public async Task<IEnumerable<MovimentacaoCaixa>> GetMovimentacoesByCaixaAsync(Guid caixaId)
        {
            var sql = @"
                SELECT
                    movimentacao_caixa_id AS MovimentacaoCaixaId,
                    caixa_id              AS CaixaId,
                    tipo                  AS Tipo,
                    descricao             AS Descricao,
                    valor                 AS Valor,
                    created_at            AS CreatedAt
                FROM movimentacao_caixa
                WHERE caixa_id = @CaixaId
                ORDER BY created_at ASC;
            ";

            return await _db.QueryAsync<MovimentacaoCaixa>(sql, new { CaixaId = caixaId });
        }
    }
}
