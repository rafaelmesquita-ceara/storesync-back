namespace SharedModels.Interfaces;

public interface ISaleRepository
{
    Task<IEnumerable<Sale>> GetAllSalesAsync();
    Task<Sale?> GetSaleByIdAsync(Guid saleId);
    Task<Guid> CreateSaleAsync(Sale sale);
    Task<int> UpdateSaleAsync(Sale sale);
    Task<int> FinalizeSaleAsync(Guid saleId);
    Task<int> CancelSaleAsync(Guid saleId);
    Task<decimal> GetTotalSalesByEmployeeAndPeriodAsync(Guid employeeId, DateTime startDate, DateTime endDate);
}

public interface ISaleService
{
    Task<IEnumerable<Sale>> GetAllSalesAsync();
    Task<Sale?> GetSaleByIdAsync(Guid saleId);
    Task<int> CreateSaleAsync(Sale sale);
    Task<int> UpdateSaleAsync(Sale sale);
    Task<int> FinalizeSaleAsync(Guid saleId);
    Task<int> CancelSaleAsync(Guid saleId);
}
