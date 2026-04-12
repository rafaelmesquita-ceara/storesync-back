namespace SharedModels.Interfaces;

public interface ISaleRepository
{
    Task<PaginatedResult<Sale>> GetAllSalesAsync(int limit = 50, int offset = 0);
    Task<Sale?> GetSaleByIdAsync(Guid saleId);
    Task<Guid> CreateSaleAsync(Sale sale);
    Task<int> UpdateSaleAsync(Sale sale);
    Task<int> FinalizeSaleAsync(Guid saleId);
    Task<int> CancelSaleAsync(Guid saleId);
    Task<decimal> GetTotalSalesByEmployeeAndPeriodAsync(Guid employeeId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Sale>> GetSalesByPeriodAsync(DateTime startDate, DateTime endDate);
}

public interface ISaleService
{
    Task<PaginatedResult<Sale>> GetAllSalesAsync(int limit = 50, int offset = 0);
    Task<Sale?> GetSaleByIdAsync(Guid saleId);
    Task<int> CreateSaleAsync(Sale sale);
    Task<int> UpdateSaleAsync(Sale sale);
    Task<int> FinalizeSaleAsync(Guid saleId);
    Task<int> CancelSaleAsync(Guid saleId);
    Task<byte[]?> DownloadSalesReportAsync(DateTime startDate, DateTime endDate);
}
