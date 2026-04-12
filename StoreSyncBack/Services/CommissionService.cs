using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Services
{
    public class CommissionService : ICommissionService
    {
        private readonly ICommissionRepository _repo;
        private readonly ISaleRepository _saleRepo;
        private readonly IEmployeeRepository _employeeRepo;

        public CommissionService(ICommissionRepository repo, ISaleRepository saleRepo, IEmployeeRepository employeeRepo)
        {
            _repo = repo;
            _saleRepo = saleRepo;
            _employeeRepo = employeeRepo;
        }

        public Task<PaginatedResult<Commission>> GetAllCommissionsAsync(int limit = 50, int offset = 0)
            => _repo.GetAllCommissionsAsync(limit, offset);

        public Task<Commission?> GetCommissionByIdAsync(Guid commissionId)
            => _repo.GetCommissionByIdAsync(commissionId);

        public async Task<(decimal TotalSales, decimal CommissionRate, decimal CommissionValue)> CalculateAsync(
            Guid employeeId, DateTime startDate, DateTime endDate)
        {
            var employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId)
                ?? throw new ArgumentException("Funcionário não encontrado.", nameof(employeeId));

            var totalSales = await _saleRepo.GetTotalSalesByEmployeeAndPeriodAsync(employeeId, startDate, endDate);
            var rate = employee.CommissionRate;
            var commissionValue = totalSales * (rate / 100m);

            return (totalSales, rate, commissionValue);
        }

        public async Task<int> CreateCommissionAsync(Commission commission)
        {
            if (commission == null)
                throw new ArgumentNullException(nameof(commission));

            if (string.IsNullOrWhiteSpace(commission.Reference))
                throw new ArgumentException("Referência é obrigatória.", nameof(commission.Reference));

            if (commission.EmployeeId == Guid.Empty)
                throw new ArgumentException("Funcionário é obrigatório.", nameof(commission.EmployeeId));

            if (commission.StartDate == default)
                throw new ArgumentException("Data inicial é obrigatória.", nameof(commission.StartDate));

            if (commission.EndDate == default)
                throw new ArgumentException("Data final é obrigatória.", nameof(commission.EndDate));

            if (commission.StartDate > commission.EndDate)
                throw new ArgumentException("Data inicial não pode ser maior que a data final.", nameof(commission.StartDate));

            var overlapping = await _repo.GetOverlappingCommissionAsync(commission.EmployeeId, commission.StartDate, commission.EndDate);
            if (overlapping != null)
                throw new InvalidOperationException(
                    $"Já existe um comissionamento para este funcionário no período informado. Referência: {overlapping.Reference}");

            var (totalSales, commissionRate, commissionValue) = await CalculateAsync(
                commission.EmployeeId, commission.StartDate, commission.EndDate);

            if (totalSales == 0)
                throw new InvalidOperationException(
                    "Nenhuma venda encontrada para o funcionário no período informado.");

            commission.TotalSales = totalSales;
            commission.CommissionRate = commissionRate;
            commission.CommissionValue = commissionValue;
            commission.CreatedAt = BrazilDateTime.Now;

            return await _repo.CreateCommissionAsync(commission);
        }

        public Task<int> DeleteCommissionAsync(Guid commissionId)
        {
            if (commissionId == Guid.Empty)
                throw new ArgumentException("CommissionId inválido.", nameof(commissionId));

            return _repo.DeleteCommissionAsync(commissionId);
        }
    }
}
