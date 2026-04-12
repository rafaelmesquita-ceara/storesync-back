namespace SharedModels.Interfaces;

public interface ICommissionRepository
{
    Task<PaginatedResult<Commission>> GetAllCommissionsAsync(int limit = 50, int offset = 0);
    Task<Commission?> GetCommissionByIdAsync(Guid commissionId);
    Task<Commission?> GetOverlappingCommissionAsync(Guid employeeId, DateTime startDate, DateTime endDate);
    Task<int> CreateCommissionAsync(Commission commission);
    Task<int> DeleteCommissionAsync(Guid commissionId);
}

public interface ICommissionService
{
    Task<PaginatedResult<Commission>> GetAllCommissionsAsync(int limit = 50, int offset = 0);
    Task<Commission?> GetCommissionByIdAsync(Guid commissionId);
    Task<(decimal TotalSales, decimal CommissionRate, decimal CommissionValue)> CalculateAsync(Guid employeeId, DateTime startDate, DateTime endDate);
    Task<int> CreateCommissionAsync(Commission commission);
    Task<int> DeleteCommissionAsync(Guid commissionId);
}
