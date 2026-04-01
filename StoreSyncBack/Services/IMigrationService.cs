namespace StoreSyncBack.Services
{
    /// <summary>
    /// Serviço responsável por aplicar migrations no banco de dados.
    /// </summary>
    public interface IMigrationService
    {
        /// <summary>
        /// Aplica todas as migrations pendentes no banco de dados.
        /// </summary>
        Task ApplyMigrationsAsync();
    }
}
