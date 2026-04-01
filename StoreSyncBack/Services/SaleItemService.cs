using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Services
{
    public class SaleItemService : ISaleItemService
    {
        private readonly ISaleItemRepository _repo;

        public SaleItemService(ISaleItemRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<SaleItem>> GetAllSaleItemsAsync()
        {
            return _repo.GetAllSaleItemsAsync();
        }

        public Task<SaleItem?> GetSaleItemByIdAsync(Guid saleItemId)
        {
            return _repo.GetSaleItemByIdAsync(saleItemId);
        }

        public async Task<int> CreateSaleItemAsync(SaleItem saleItem)
        {
            if (saleItem == null)
                throw new ArgumentNullException(nameof(saleItem));

            if (saleItem.SaleId == Guid.Empty)
                throw new ArgumentException("SaleId é obrigatório.", nameof(saleItem.SaleId));

            if (saleItem.ProductId == Guid.Empty)
                throw new ArgumentException("ProductId é obrigatório.", nameof(saleItem.ProductId));

            if (saleItem.Quantity <= 0)
                throw new ArgumentException("Quantity deve ser maior que zero.", nameof(saleItem.Quantity));

            if (saleItem.TotalPrice < 0)
                throw new ArgumentException("TotalPrice inválido.", nameof(saleItem.TotalPrice));

            if (saleItem.SaleItemId == Guid.Empty)
                saleItem.SaleItemId = Guid.NewGuid();

            if (saleItem.CreatedAt == default)
                saleItem.CreatedAt = DateTime.UtcNow;

            // Se TotalPrice não foi fornecido, deixamos o repositório tentar calcular usando Product.Price
            return await _repo.CreateSaleItemAsync(saleItem);
        }

        public async Task<int> UpdateSaleItemAsync(SaleItem saleItem)
        {
            if (saleItem == null)
                throw new ArgumentNullException(nameof(saleItem));

            if (saleItem.SaleItemId == Guid.Empty)
                throw new ArgumentException("SaleItemId inválido.", nameof(saleItem.SaleItemId));

            if (saleItem.Quantity <= 0)
                throw new ArgumentException("Quantity deve ser maior que zero.", nameof(saleItem.Quantity));

            if (saleItem.TotalPrice < 0)
                throw new ArgumentException("TotalPrice inválido.", nameof(saleItem.TotalPrice));

            return await _repo.UpdateSaleItemAsync(saleItem);
        }

        public Task<int> DeleteSaleItemAsync(Guid saleItemId)
        {
            if (saleItemId == Guid.Empty)
                throw new ArgumentException("SaleItemId inválido.", nameof(saleItemId));

            return _repo.DeleteSaleItemAsync(saleItemId);
        }
    }
}
