using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Services
{
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _repo;

        public SaleService(ISaleRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<Sale>> GetAllSalesAsync()
        {
            return _repo.GetAllSalesAsync();
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

            return await _repo.FinalizeSaleAsync(saleId);
        }

        public async Task<int> CancelSaleAsync(Guid saleId)
        {
            if (saleId == Guid.Empty)
                throw new ArgumentException("SaleId inválido.", nameof(saleId));

            return await _repo.CancelSaleAsync(saleId);
        }
    }
}
