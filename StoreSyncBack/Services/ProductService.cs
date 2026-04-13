using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository repo)
        {
            _repo = repo;
        }

        public Task<PaginatedResult<Product>> GetAllProductsAsync(int limit = 50, int offset = 0)
        {
            return _repo.GetAllProductsAsync(limit, offset);
        }

        public Task<Product?> GetProductByIdAsync(Guid productId)
        {
            return _repo.GetProductByIdAsync(productId);
        }

        public async Task<int> CreateProductAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException("Name é obrigatório.", nameof(product.Name));

            if (product.Price < 0)
                throw new ArgumentException("Price não pode ser negativo.", nameof(product.Price));

            if (product.CostPrice < 0)
                throw new ArgumentException("CostPrice não pode ser negativo.", nameof(product.CostPrice));

            if (product.StockQuantity < 0)
                throw new ArgumentException("StockQuantity não pode ser negativo.", nameof(product.StockQuantity));

            if (product.ProductId == Guid.Empty)
                product.ProductId = Guid.NewGuid();

            if (product.CreatedAt == default)
                product.CreatedAt = BrazilDateTime.Now;

            return await _repo.CreateProductAsync(product);
        }

        public async Task<int> UpdateProductAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (product.ProductId == Guid.Empty)
                throw new ArgumentException("ProductId inválido.", nameof(product.ProductId));

            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException("Name é obrigatório.", nameof(product.Name));

            if (product.Price < 0)
                throw new ArgumentException("Price não pode ser negativo.", nameof(product.Price));

            if (product.CostPrice < 0)
                throw new ArgumentException("CostPrice não pode ser negativo.", nameof(product.CostPrice));

            if (product.StockQuantity < 0)
                throw new ArgumentException("StockQuantity não pode ser negativo.", nameof(product.StockQuantity));

            return await _repo.UpdateProductAsync(product);
        }

        public Task<int> DeleteProductAsync(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("ProductId inválido.", nameof(productId));

            return _repo.DeleteProductAsync(productId);
        }
    }
}
