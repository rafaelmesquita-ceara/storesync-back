using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Services
{
    public class SalePaymentService : ISalePaymentService
    {
        private readonly ISalePaymentRepository _repo;
        private readonly ISaleRepository _saleRepo;
        private readonly IPaymentMethodRepository _pmRepo;

        public SalePaymentService(
            ISalePaymentRepository repo,
            ISaleRepository saleRepo,
            IPaymentMethodRepository pmRepo)
        {
            _repo = repo;
            _saleRepo = saleRepo;
            _pmRepo = pmRepo;
        }

        public Task<PaginatedResult<SalePayment>> GetAllSalePaymentsAsync(int limit = 50, int offset = 0)
        {
            return _repo.GetAllSalePaymentsAsync(limit, offset);
        }

        public Task<IEnumerable<SalePayment>> GetBySaleIdAsync(Guid saleId)
        {
            return _repo.GetBySaleIdAsync(saleId);
        }

        public async Task<int> AddPaymentAsync(SalePayment payment)
        {
            if (payment.Amount <= 0)
                throw new ArgumentException("O valor do pagamento deve ser maior que zero.", nameof(payment.Amount));

            var sale = await _saleRepo.GetSaleByIdAsync(payment.SaleId);
            if (sale == null)
                throw new ArgumentException("Venda não encontrada.");

            if (sale.Status != SaleStatus.Aberta)
                throw new InvalidOperationException("Pagamentos só podem ser adicionados a vendas em aberto.");

            var pm = await _pmRepo.GetByIdAsync(payment.PaymentMethodId);
            if (pm == null)
                throw new ArgumentException("Forma de pagamento não encontrada.");

            if (pm.Type == PaymentMethodType.Cash || pm.Type == PaymentMethodType.Pix)
            {
                payment.Installments = 1;
                payment.SurchargeApplied = false;
                payment.SurchargeAmount = 0;
            }

            if (payment.SurchargeApplied && payment.SurchargeAmount > 0)
            {
                sale.Addition += payment.SurchargeAmount;
                await _saleRepo.UpdateSaleAsync(sale);
            }

            await _repo.CreateAsync(payment);
            return 1;
        }

        public async Task<int> RemovePaymentAsync(Guid salePaymentId)
        {
            var payment = await _repo.GetByIdAsync(salePaymentId);
            if (payment == null)
                throw new ArgumentException("Pagamento não encontrado.");

            var sale = await _saleRepo.GetSaleByIdAsync(payment.SaleId);
            if (sale == null)
                throw new ArgumentException("Venda não encontrada.");

            if (sale.Status != SaleStatus.Aberta)
                throw new InvalidOperationException("Pagamentos só podem ser removidos de vendas em aberto.");

            if (payment.SurchargeApplied && payment.SurchargeAmount > 0)
            {
                sale.Addition = Math.Max(0, sale.Addition - payment.SurchargeAmount);
                await _saleRepo.UpdateSaleAsync(sale);
            }

            return await _repo.DeleteAsync(salePaymentId);
        }
    }
}
