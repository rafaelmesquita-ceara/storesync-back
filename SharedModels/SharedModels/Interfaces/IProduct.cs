namespace SharedModels.Interfaces;

public interface IProductRepository
{
    Task<PaginatedResult<Product>> GetAllProductsAsync(int limit = 50, int offset = 0);
    Task<Product?> GetProductByIdAsync(Guid productId);
    Task<int> CreateProductAsync(Product product);
    Task<int> UpdateProductAsync(Product product);
    Task<int> DeleteProductAsync(Guid productId);
}

public interface IProductService
{
    Task<PaginatedResult<Product>> GetAllProductsAsync(int limit = 50, int offset = 0);
    Task<Product?> GetProductByIdAsync(Guid productId);
    Task<int> CreateProductAsync(Product product);
    Task<int> UpdateProductAsync(Product product);
    Task<int> DeleteProductAsync(Guid productId);
}
