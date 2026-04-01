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

            if (sale.Items == null || sale.Items.Count == 0)
                throw new ArgumentException("A venda deve conter pelo menos um item.", nameof(sale.Items));

            // Calcula total automaticamente, se necessário
            sale.TotalAmount = sale.Items.Sum(i =>
                i.TotalPrice > 0
                    ? i.TotalPrice
                    : i.Quantity * (i.Product?.Price ?? 0));

            await _repo.CreateSaleAsync(sale);
            return 1;
        }

        public async Task<int> UpdateSaleAsync(Sale sale)
        {
            if (sale == null)
                throw new ArgumentNullException(nameof(sale));

            if (sale.SaleId == Guid.Empty)
                throw new ArgumentException("SaleId inválido.", nameof(sale.SaleId));

            return await _repo.UpdateSaleAsync(sale);
        }

        public Task<int> DeleteSaleAsync(Guid saleId)
        {
            if (saleId == Guid.Empty)
                throw new ArgumentException("SaleId inválido.", nameof(saleId));

            return _repo.DeleteSaleAsync(saleId);
        }
    }
}
