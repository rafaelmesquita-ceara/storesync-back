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
            => _repo.GetAllFinanceAsync();

        public Task<IEnumerable<Finance>> GetAllByTypeAsync(int type)
            => _repo.GetAllByTypeAsync(type);

        public Task<Finance?> GetFinanceByIdAsync(Guid financeId)
            => _repo.GetFinanceByIdAsync(financeId);

        public async Task<int> CreateFinanceAsync(Finance finance)
        {
            if (finance == null)
                throw new ArgumentNullException(nameof(finance));

            if (string.IsNullOrWhiteSpace(finance.Description))
                throw new ArgumentException("Descrição é obrigatória.", nameof(finance.Description));

            if (finance.Amount <= 0)
                throw new ArgumentException("Valor deve ser maior que zero.", nameof(finance.Amount));

            if (finance.DueDate == default)
                throw new ArgumentException("Data de vencimento é obrigatória.", nameof(finance.DueDate));

            if (finance.Status == 0)
                finance.Status = FinanceStatus.Aberto;

            if (finance.Type == 0)
                finance.Type = FinanceType.Pagar;

            if (finance.TitleType == 0)
                finance.TitleType = FinanceTitleType.Original;

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
                throw new ArgumentException("Valor deve ser maior que zero.", nameof(finance.Amount));

            return await _repo.UpdateFinanceAsync(finance);
        }

        public async Task<int> DeleteFinanceAsync(Guid financeId)
        {
            if (financeId == Guid.Empty)
                throw new ArgumentException("FinanceId inválido.", nameof(financeId));

            var finance = await _repo.GetFinanceByIdAsync(financeId);
            if (finance == null)
                throw new KeyNotFoundException("Registro financeiro não encontrado.");

            if (finance.Status != FinanceStatus.Aberto)
                throw new InvalidOperationException("Apenas registros em aberto podem ser excluídos.");

            return await _repo.DeleteFinanceAsync(financeId);
        }

        public async Task SettleAsync(Guid financeId, decimal settledAmount, string? note)
        {
            if (financeId == Guid.Empty)
                throw new ArgumentException("FinanceId inválido.", nameof(financeId));

            var finance = await _repo.GetFinanceByIdAsync(financeId);
            if (finance == null)
                throw new KeyNotFoundException("Registro financeiro não encontrado.");

            if (settledAmount <= 0)
                throw new ArgumentException("O valor liquidado deve ser maior que zero.");

            if (settledAmount > finance.Amount)
                throw new InvalidOperationException("O valor liquidado não pode ser maior que o valor da conta.");

            var settledAt = DateTime.UtcNow;

            if (settledAmount == finance.Amount)
            {
                await _repo.SettleAsync(financeId, settledAmount, settledAt, note, FinanceStatus.Liquidado);
            }
            else
            {
                await _repo.SettleAsync(financeId, settledAmount, settledAt, note, FinanceStatus.LiquidadoParcialmente);

                var residual = new Finance
                {
                    Amount      = finance.Amount - settledAmount,
                    Description = finance.Description,
                    Reference   = $"Residual de {finance.Reference}",
                    DueDate     = finance.DueDate,
                    Type        = finance.Type,
                    TitleType   = FinanceTitleType.Residual,
                    Status      = FinanceStatus.Aberto,
                    ParentId    = financeId,
                    CreatedAt   = DateTime.UtcNow
                };

                await _repo.CreateFinanceAsync(residual);
            }
        }

        public async Task CancelSettlementAsync(Guid financeId)
        {
            if (financeId == Guid.Empty)
                throw new ArgumentException("FinanceId inválido.", nameof(financeId));

            var finance = await _repo.GetFinanceByIdAsync(financeId);
            if (finance == null)
                throw new KeyNotFoundException("Registro financeiro não encontrado.");

            if (finance.Status == FinanceStatus.Aberto)
                throw new InvalidOperationException("Este registro já está em aberto.");

            if (finance.Status == FinanceStatus.LiquidadoParcialmente)
            {
                var residual = await _repo.GetResidualByParentIdAsync(financeId);
                if (residual != null)
                    throw new InvalidOperationException(
                        "Não é possível cancelar a liquidação pois existe um registro residual vinculado. " +
                        "Exclua o registro residual primeiro e tente novamente.");
            }

            await _repo.CancelSettlementAsync(financeId);
        }
    }
}
