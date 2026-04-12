namespace SharedModels.Interfaces;

public interface IPaymentMethodRepository
{
    Task<IEnumerable<PaymentMethod>> GetAllAsync();
    Task<PaymentMethod?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(PaymentMethod pm);
    Task<int> UpdateAsync(PaymentMethod pm);
    Task<int> DeleteAsync(Guid id);
    Task<bool> IsUsedInSalesAsync(Guid id);
    Task<IEnumerable<PaymentMethodRate>> GetRatesByMethodIdAsync(Guid methodId);
    Task<Guid> AddRateAsync(PaymentMethodRate rate);
    Task<int> DeleteRateAsync(Guid rateId);
}

public interface IPaymentMethodService
{
    Task<IEnumerable<PaymentMethod>> GetAllAsync();
    Task<PaymentMethod?> GetByIdAsync(Guid id);
    Task<int> CreateAsync(PaymentMethod pm);
    Task<int> UpdateAsync(PaymentMethod pm);
    Task<int> DeleteAsync(Guid id);
    Task<int> AddRateAsync(Guid methodId, PaymentMethodRate rate);
    Task<int> DeleteRateAsync(Guid methodId, Guid rateId);
}
