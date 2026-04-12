namespace SharedModels.Interfaces;

public interface ICaixaRepository
{
    Task<PaginatedResult<Caixa>> GetAllAsync(int limit, int offset);
    Task<Caixa?> GetByIdAsync(Guid id);
    Task<Caixa?> GetCaixaAbertoAsync();
    Task<Guid> CreateAsync(Caixa caixa);
    Task<int> FecharAsync(Guid id, decimal valorFechamento, decimal totalVendas, decimal totalSangrias, decimal totalSuprimentos, decimal? valorFaltante, decimal? valorSobra);
    Task<int> AddMovimentacaoAsync(MovimentacaoCaixa mov);
    Task<IEnumerable<Sale>> GetVendasByCaixaAsync(Guid caixaId);
    Task<IEnumerable<MovimentacaoCaixa>> GetMovimentacoesByCaixaAsync(Guid caixaId);
}

public interface ICaixaService
{
    Task<PaginatedResult<Caixa>> GetAllAsync(int limit, int offset);
    Task<Caixa?> GetByIdAsync(Guid id);
    Task<Caixa?> GetCaixaAbertoAsync();
    Task<Caixa> AbrirCaixaAsync(decimal valorAbertura);
    Task FecharCaixaAsync(Guid id, decimal valorFechamento);
    Task AddMovimentacaoAsync(Guid caixaId, int tipo, string? descricao, decimal valor);
    Task<byte[]> GerarRelatorioPdfAsync(Guid caixaId);
}
