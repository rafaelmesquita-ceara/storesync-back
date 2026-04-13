using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Services
{
    public class SaleItemService : ISaleItemService
    {
        private readonly ISaleItemRepository _repo;
        private readonly ISaleRepository _saleRepo;
        private readonly IProductRepository _productRepo;

        public SaleItemService(ISaleItemRepository repo, ISaleRepository saleRepo, IProductRepository productRepo)
        {
            _repo = repo;
            _saleRepo = saleRepo;
            _productRepo = productRepo;
        }

        public Task<PaginatedResult<SaleItem>> GetAllSaleItemsAsync(int limit = 50, int offset = 0)
        {
            return _repo.GetAllSaleItemsAsync(limit, offset);
        }

        public Task<PaginatedResult<SaleItem>> GetSaleItemsBySaleIdAsync(Guid saleId, int limit = 50, int offset = 0)
        {
            return _repo.GetSaleItemsBySaleIdAsync(saleId, limit, offset);
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

            var sale = await _saleRepo.GetSaleByIdAsync(saleItem.SaleId);
            if (sale == null)
                throw new ArgumentException("Venda não encontrada.");
            if (sale.Status != SaleStatus.Aberta)
                throw new InvalidOperationException("Apenas vendas em aberto permitem adicionar itens.");

            var product = await _productRepo.GetProductByIdAsync(saleItem.ProductId);
            if (product == null)
                throw new ArgumentException("Produto não encontrado.");
            if (saleItem.Quantity > product.StockQuantity)
                throw new InvalidOperationException($"Estoque insuficiente. Disponível: {product.StockQuantity}.");

            saleItem.CostPrice = product.CostPrice;
            saleItem.TotalPrice = (saleItem.Quantity * product.Price) - saleItem.Discount + saleItem.Addition;

            if (saleItem.SaleItemId == Guid.Empty)
                saleItem.SaleItemId = Guid.NewGuid();

            if (saleItem.CreatedAt == default)
                saleItem.CreatedAt = BrazilDateTime.Now;

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

            return await _repo.UpdateSaleItemAsync(saleItem);
        }

        public async Task<int> DeleteSaleItemAsync(Guid saleItemId)
        {
            if (saleItemId == Guid.Empty)
                throw new ArgumentException("SaleItemId inválido.", nameof(saleItemId));

            var item = await _repo.GetSaleItemByIdAsync(saleItemId);
            if (item != null)
            {
                var sale = await _saleRepo.GetSaleByIdAsync(item.SaleId);
                if (sale != null && sale.Status != SaleStatus.Aberta)
                    throw new InvalidOperationException("Apenas vendas em aberto permitem remover itens.");
            }

            return await _repo.DeleteSaleItemAsync(saleItemId);
        }
    }
}
