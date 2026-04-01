namespace SharedModels.Interfaces;

public interface IFinanceRepository
{
    Task<IEnumerable<Finance>> GetAllFinanceAsync();
    Task<Finance?> GetFinanceByIdAsync(Guid financeId);
    Task<int> CreateFinanceAsync(Finance finance);
    Task<int> UpdateFinanceAsync(Finance finance);
    Task<int> DeleteFinanceAsync(Guid financeId);
}

public interface IFinanceService
{
    Task<IEnumerable<Finance>> GetAllFinanceAsync();
    Task<Finance?> GetFinanceByIdAsync(Guid financeId);
    Task<int> CreateFinanceAsync(Finance finance);
    Task<int> UpdateFinanceAsync(Finance finance);
    Task<int> DeleteFinanceAsync(Guid financeId);
}
