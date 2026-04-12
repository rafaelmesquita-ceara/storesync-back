namespace SharedModels.Interfaces;

public interface ISalePaymentRepository
{
    Task<IEnumerable<SalePayment>> GetBySaleIdAsync(Guid saleId);
    Task<SalePayment?> GetByIdAsync(Guid salePaymentId);
    Task<Guid> CreateAsync(SalePayment payment);
    Task<int> DeleteAsync(Guid salePaymentId);
    Task<decimal> GetTotalPaidBySaleIdAsync(Guid saleId);
}

public interface ISalePaymentService
{
    Task<IEnumerable<SalePayment>> GetBySaleIdAsync(Guid saleId);
    Task<int> AddPaymentAsync(SalePayment payment);
    Task<int> RemovePaymentAsync(Guid salePaymentId);
}
