using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Services
{
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _repo;
        private readonly ISalePaymentRepository _salePaymentRepo;

        public SaleService(ISaleRepository repo, ISalePaymentRepository salePaymentRepo)
        {
            _repo = repo;
            _salePaymentRepo = salePaymentRepo;
        }

        public Task<PaginatedResult<Sale>> GetAllSalesAsync(int limit = 50, int offset = 0)
        {
            return _repo.GetAllSalesAsync(limit, offset);
        }

        public Task<Sale?> GetSaleByIdAsync(Guid saleId)
        {
            return _repo.GetSaleByIdAsync(saleId);
        }

        public async Task<int> CreateSaleAsync(Sale sale)
        {
            if (sale == null)
                throw new ArgumentNullException(nameof(sale));

            if (sale.EmployeeId == Guid.Empty)
                throw new ArgumentException("EmployeeId é obrigatório.", nameof(sale.EmployeeId));

            sale.TotalAmount = 0;
            sale.Status = SaleStatus.Aberta;

            await _repo.CreateSaleAsync(sale);
            return 1;
        }

        public async Task<int> UpdateSaleAsync(Sale sale)
        {
            if (sale == null)
                throw new ArgumentNullException(nameof(sale));

            if (sale.SaleId == Guid.Empty)
                throw new ArgumentException("SaleId inválido.", nameof(sale.SaleId));

            var existing = await _repo.GetSaleByIdAsync(sale.SaleId);
            if (existing == null)
                throw new ArgumentException("Venda não encontrada.");
            if (existing.Status != SaleStatus.Aberta)
                throw new InvalidOperationException("Apenas vendas em aberto podem ser editadas.");

            return await _repo.UpdateSaleAsync(sale);
        }

        public async Task<int> FinalizeSaleAsync(Guid saleId)
        {
            if (saleId == Guid.Empty)
                throw new ArgumentException("SaleId inválido.", nameof(saleId));

            var sale = await _repo.GetSaleByIdAsync(saleId);
            if (sale == null)
                throw new ArgumentException("Venda não encontrada.");
            if (sale.Status != SaleStatus.Aberta)
                throw new InvalidOperationException("Apenas vendas em aberto podem ser finalizadas.");
            if (sale.Items == null || sale.Items.Count == 0)
                throw new InvalidOperationException("A venda deve conter pelo menos um item para ser finalizada.");

            var totalPaid = await _salePaymentRepo.GetTotalPaidBySaleIdAsync(saleId);
            if (totalPaid < sale.TotalAmount)
                throw new InvalidOperationException(
                    $"Pagamento insuficiente. Total a pagar: R$ {sale.TotalAmount:N2}. Total pago: R$ {totalPaid:N2}.");

            var troco = totalPaid - sale.TotalAmount;
            return await _repo.FinalizeSaleAsync(saleId, troco);
        }

        public async Task<int> CancelSaleAsync(Guid saleId)
        {
            if (saleId == Guid.Empty)
                throw new ArgumentException("SaleId inválido.", nameof(saleId));

            return await _repo.CancelSaleAsync(saleId);
        }

        public Task<byte[]?> DownloadSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }
    }
}
