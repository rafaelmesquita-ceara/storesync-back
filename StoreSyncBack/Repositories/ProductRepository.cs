using System.Data;
using Dapper;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbConnection _db;

        public ProductRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<PaginatedResult<Product>> GetAllProductsAsync(int limit = 50, int offset = 0)
        {
            var countSql = "SELECT COUNT(*) FROM product;";
            var totalCount = await _db.ExecuteScalarAsync<int>(countSql);

            var sql = @"
                SELECT
                    p.product_id AS ProductId,
                    p.reference AS Reference,
                    p.name AS Name,
                    p.category_id AS CategoryId,
                    p.price AS Price,
                    p.cost_price AS CostPrice,
                    p.stock_quantity AS StockQuantity,
                    p.created_at AS CreatedAt,
                    TRUE AS _splitCategory,
                    c.category_id AS CategoryId,
                    c.name AS Name,
                    c.created_at AS CreatedAt
                FROM product p
                LEFT JOIN category c ON p.category_id = c.category_id
                ORDER BY p.name
                LIMIT @Limit OFFSET @Offset;
            ";

            var result = await _db.QueryAsync<Product, Category, Product>(
                sql,
                (product, category) =>
                {
                    if (category != null && category.CategoryId != Guid.Empty)
                        product.Category = category;
                    return product;
                },
                new { Limit = limit, Offset = offset },
                splitOn: "_splitCategory"
            );

            return new PaginatedResult<Product>
            {
                Items = result,
                TotalCount = totalCount,
                Limit = limit,
                Offset = offset
            };
        }

        public async Task<Product?> GetProductByIdAsync(Guid productId)
        {
            var sql = @"
                SELECT
                    p.product_id AS ProductId,
                    p.reference AS Reference,
                    p.name AS Name,
                    p.category_id AS CategoryId,
                    p.price AS Price,
                    p.cost_price AS CostPrice,
                    p.stock_quantity AS StockQuantity,
                    p.created_at AS CreatedAt,
                    TRUE AS _splitCategory,
                    c.category_id AS CategoryId,
                    c.name AS Name,
                    c.created_at AS CreatedAt
                FROM product p
                LEFT JOIN category c ON p.category_id = c.category_id
                WHERE p.product_id = @Id;
            ";

            var result = await _db.QueryAsync<Product, Category, Product>(
                sql,
                (product, category) =>
                {
                    if (category != null && category.CategoryId != Guid.Empty)
                        product.Category = category;
                    return product;
                },
                new { Id = productId },
                splitOn: "_splitCategory"
            );
            return result.FirstOrDefault();
        }

        public async Task<int> CreateProductAsync(Product product)
        {
            if (product.ProductId == Guid.Empty)
                product.ProductId = Guid.NewGuid();

            if (product.CreatedAt == default)
                product.CreatedAt = BrazilDateTime.Now;

            var sql = @"
                INSERT INTO product (product_id, reference, name, category_id, price, cost_price, stock_quantity, created_at)
                VALUES (@ProductId, @Reference, @Name, @CategoryId, @Price, @CostPrice, @StockQuantity, @CreatedAt);
            ";

            var affected = await _db.ExecuteAsync(sql, new
            {
                product.ProductId,
                product.Reference,
                product.Name,
                CategoryId = product.CategoryId,
                product.Price,
                product.CostPrice,
                product.StockQuantity,
                product.CreatedAt
            });

            return affected;
        }

        public async Task<int> UpdateProductAsync(Product product)
        {
            var sql = @"
                UPDATE product
                SET
                    reference = @Reference,
                    name = @Name,
                    category_id = @CategoryId,
                    price = @Price,
                    cost_price = @CostPrice,
                    stock_quantity = @StockQuantity
                WHERE product_id = @ProductId;
            ";

            var affected = await _db.ExecuteAsync(sql, new
            {
                product.Reference,
                product.Name,
                CategoryId = product.CategoryId,
                product.Price,
                product.CostPrice,
                product.StockQuantity,
                product.ProductId
            });

            return affected;
        }

        public async Task<int> DeleteProductAsync(Guid productId)
        {
            var sql = "DELETE FROM product WHERE product_id = @Id;";
            var affected = await _db.ExecuteAsync(sql, new { Id = productId });
            return affected;
        }
    }
}
