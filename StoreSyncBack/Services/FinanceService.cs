using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Services
{
    public class FinanceService : IFinanceService
    {
        private readonly IFinanceRepository _repo;

        public FinanceService(IFinanceRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<Finance>> GetAllFinanceAsync()
        {
            return _repo.GetAllFinanceAsync();
        }

        public Task<Finance?> GetFinanceByIdAsync(Guid financeId)
        {
            return _repo.GetFinanceByIdAsync(financeId);
        }

        public async Task<int> CreateFinanceAsync(Finance finance)
        {
            if (finance == null)
                throw new ArgumentNullException(nameof(finance));

            if (string.IsNullOrWhiteSpace(finance.Description))
                throw new ArgumentException("Description é obrigatória.", nameof(finance.Description));

            if (finance.Amount <= 0)
                throw new ArgumentException("Amount deve ser maior que zero.", nameof(finance.Amount));

            if (finance.DueDate == default)
                throw new ArgumentException("DueDate é obrigatório.", nameof(finance.DueDate));

            if (string.IsNullOrWhiteSpace(finance.Status))
                finance.Status = "Pendente"; // valor padrão

            if (finance.CreatedAt == default)
                finance.CreatedAt = DateTime.UtcNow;

            return await _repo.CreateFinanceAsync(finance);
        }

        public async Task<int> UpdateFinanceAsync(Finance finance)
        {
            if (finance == null)
                throw new ArgumentNullException(nameof(finance));

            if (finance.FinanceId == Guid.Empty)
                throw new ArgumentException("FinanceId inválido.", nameof(finance.FinanceId));

            if (finance.Amount <= 0)
                throw new ArgumentException("Amount deve ser maior que zero.", nameof(finance.Amount));

            return await _repo.UpdateFinanceAsync(finance);
        }

        public Task<int> DeleteFinanceAsync(Guid financeId)
        {
            if (financeId == Guid.Empty)
                throw new ArgumentException("FinanceId inválido.", nameof(financeId));

            return _repo.DeleteFinanceAsync(financeId);
        }
    }
}
