namespace SharedModels.Interfaces;

public interface IFinanceRepository
{
    Task<PaginatedResult<Finance>> GetAllFinanceAsync(int limit = 50, int offset = 0);
    Task<PaginatedResult<Finance>> GetAllByTypeAsync(int type, int limit = 50, int offset = 0);
    Task<Finance?> GetFinanceByIdAsync(Guid financeId);
    Task<Finance?> GetResidualByParentIdAsync(Guid parentId);
    Task<int> CreateFinanceAsync(Finance finance);
    Task<int> UpdateFinanceAsync(Finance finance);
    Task<int> DeleteFinanceAsync(Guid financeId);
    Task<int> SettleAsync(Guid financeId, decimal settledAmount, DateTime settledAt, string? settledNote, int status);
    Task<int> CancelSettlementAsync(Guid financeId);
}

public interface IFinanceService
{
    Task<PaginatedResult<Finance>> GetAllFinanceAsync(int limit = 50, int offset = 0);
    Task<PaginatedResult<Finance>> GetAllByTypeAsync(int type, int limit = 50, int offset = 0);
    Task<Finance?> GetFinanceByIdAsync(Guid financeId);
    Task<int> CreateFinanceAsync(Finance finance);
    Task<int> UpdateFinanceAsync(Finance finance);
    Task<int> DeleteFinanceAsync(Guid financeId);
    Task SettleAsync(Guid financeId, decimal settledAmount, string? note);
    Task CancelSettlementAsync(Guid financeId);
}
