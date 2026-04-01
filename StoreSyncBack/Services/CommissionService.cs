using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Services
{
    public class CommissionService : ICommissionService
    {
        private readonly ICommissionRepository _repo;

        public CommissionService(ICommissionRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<Commission>> GetAllCommissionsAsync()
        {
            return _repo.GetAllCommissionsAsync();
        }

        public Task<Commission?> GetCommissionByIdAsync(Guid commissionId)
        {
            return _repo.GetCommissionByIdAsync(commissionId);
        }

        public async Task<int> CreateCommissionAsync(Commission commission)
        {
            if (commission == null)
                throw new ArgumentNullException(nameof(commission));

            if (commission.EmployeeId == Guid.Empty)
                throw new ArgumentException("EmployeeId é obrigatório.", nameof(commission.EmployeeId));

            if (commission.Month == default)
                throw new ArgumentException("Month é obrigatório.", nameof(commission.Month));

            if (commission.TotalSales < 0)
                throw new ArgumentException("TotalSales não pode ser negativo.", nameof(commission.TotalSales));

            if (commission.CommissionValue < 0)
                throw new ArgumentException("CommissionValue não pode ser negativo.", nameof(commission.CommissionValue));

            if (commission.CreatedAt == default)
                commission.CreatedAt = DateTime.UtcNow;

            return await _repo.CreateCommissionAsync(commission);
        }

        public async Task<int> UpdateCommissionAsync(Commission commission)
        {
            if (commission == null)
                throw new ArgumentNullException(nameof(commission));

            if (commission.CommissionId == null || commission.CommissionId == Guid.Empty)
                throw new ArgumentException("CommissionId inválido.", nameof(commission.CommissionId));

            if (commission.EmployeeId == Guid.Empty)
                throw new ArgumentException("EmployeeId é obrigatório.", nameof(commission.EmployeeId));

            return await _repo.UpdateCommissionAsync(commission);
        }

        public Task<int> DeleteCommissionAsync(Guid commissionId)
        {
            if (commissionId == Guid.Empty)
                throw new ArgumentException("CommissionId inválido.", nameof(commissionId));

            return _repo.DeleteCommissionAsync(commissionId);
        }
    }
}
