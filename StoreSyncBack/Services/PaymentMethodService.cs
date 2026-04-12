using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Services
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly IPaymentMethodRepository _repo;

        public PaymentMethodService(IPaymentMethodRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<PaymentMethod>> GetAllAsync()
        {
            return _repo.GetAllAsync();
        }

        public Task<PaymentMethod?> GetByIdAsync(Guid id)
        {
            return _repo.GetByIdAsync(id);
        }

        public async Task<int> CreateAsync(PaymentMethod pm)
        {
            if (string.IsNullOrWhiteSpace(pm.Name))
                throw new ArgumentException("O nome da forma de pagamento é obrigatório.", nameof(pm.Name));

            if (pm.Type < 1 || pm.Type > 4)
                throw new ArgumentException("Tipo de forma de pagamento inválido. Use: 1=Dinheiro, 2=Débito, 3=Crédito, 4=Pix.", nameof(pm.Type));

            if (pm.Rates != null && pm.Rates.Count > 0 &&
                (pm.Type == PaymentMethodType.Cash || pm.Type == PaymentMethodType.Pix))
            {
                throw new InvalidOperationException("Formas de pagamento do tipo Dinheiro e Pix não permitem taxas.");
            }

            await _repo.CreateAsync(pm);

            if (pm.Rates != null)
            {
                foreach (var rate in pm.Rates)
                {
                    rate.PaymentMethodId = pm.PaymentMethodId;
                    await _repo.AddRateAsync(rate);
                }
            }

            return 1;
        }

        public async Task<int> UpdateAsync(PaymentMethod pm)
        {
            var existing = await _repo.GetByIdAsync(pm.PaymentMethodId);
            if (existing == null)
                throw new ArgumentException("Forma de pagamento não encontrada.");

            if ((pm.Type == PaymentMethodType.Cash || pm.Type == PaymentMethodType.Pix) &&
                existing.Rates != null && existing.Rates.Count > 0)
            {
                throw new InvalidOperationException("Não é possível alterar para tipo Dinheiro ou Pix enquanto existirem taxas cadastradas. Remova as taxas primeiro.");
            }

            return await _repo.UpdateAsync(pm);
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var inUse = await _repo.IsUsedInSalesAsync(id);
            if (inUse)
                throw new InvalidOperationException("Esta forma de pagamento está associada a vendas e não pode ser excluída. Você pode inativá-la.");

            return await _repo.DeleteAsync(id);
        }

        public async Task<int> AddRateAsync(Guid methodId, PaymentMethodRate rate)
        {
            var pm = await _repo.GetByIdAsync(methodId);
            if (pm == null)
                throw new ArgumentException("Forma de pagamento não encontrada.");

            if (pm.Type == PaymentMethodType.Cash || pm.Type == PaymentMethodType.Pix)
                throw new InvalidOperationException("Formas de pagamento do tipo Dinheiro e Pix não permitem taxas.");

            if (rate.Installments < 1)
                throw new ArgumentException("O número de parcelas deve ser maior ou igual a 1.", nameof(rate.Installments));

            if (rate.RatePercentage < 0)
                throw new ArgumentException("A taxa percentual não pode ser negativa.", nameof(rate.RatePercentage));

            var existingRates = pm.Rates ?? new List<PaymentMethodRate>();
            if (existingRates.Any(r => r.Installments == rate.Installments))
                throw new InvalidOperationException($"Já existe uma taxa cadastrada para {rate.Installments} parcela(s) nesta forma de pagamento.");

            rate.PaymentMethodId = methodId;
            await _repo.AddRateAsync(rate);
            return 1;
        }

        public async Task<int> DeleteRateAsync(Guid methodId, Guid rateId)
        {
            return await _repo.DeleteRateAsync(rateId);
        }
    }
}
