namespace SharedModels.Interfaces;

public interface ICommissionRepository
{
    Task<IEnumerable<Commission>> GetAllCommissionsAsync();
    Task<Commission?> GetCommissionByIdAsync(Guid commissionId);
    Task<int> CreateCommissionAsync(Commission commission);
    Task<int> UpdateCommissionAsync(Commission commission);
    Task<int> DeleteCommissionAsync(Guid commissionId);
}

public interface ICommissionService
{
    Task<IEnumerable<Commission>> GetAllCommissionsAsync();
    Task<Commission?> GetCommissionByIdAsync(Guid commissionId);
    Task<int> CreateCommissionAsync(Commission commission);
    Task<int> UpdateCommissionAsync(Commission commission);
    Task<int> DeleteCommissionAsync(Guid commissionId);
}
