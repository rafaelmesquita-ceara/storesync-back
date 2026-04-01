using System.Data;
using Dapper;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Repositories
{
    public class SaleItemRepository : ISaleItemRepository
    {
        private readonly IDbConnection _db;

        public SaleItemRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<SaleItem>> GetAllSaleItemsAsync()
        {
            var sql = @"
                SELECT
                    si.sale_item_id AS SaleItemId,
                    si.sale_id AS SaleId,
                    si.product_id AS ProductId,
                    si.quantity AS Quantity,
                    si.total_price AS TotalPrice,
                    si.created_at AS CreatedAt,
                    p.product_id AS ProductId,
                    p.reference AS Reference,
                    p.name AS Name,
                    p.category_id AS CategoryId,
                    p.price AS Price,
                    p.stock_quantity AS StockQuantity,
                    p.created_at AS CreatedAt
                FROM sale_item si
                LEFT JOIN product p ON si.product_id = p.product_id
                ORDER BY si.created_at DESC;
            ";

            var result = await _db.QueryAsync<SaleItem, Product, SaleItem>(
                sql,
                (saleItem, product) =>
                {
                    saleItem.Product = product;
                    return saleItem;
                },
                splitOn: "ProductId"
            );

            return result;
        }

        public async Task<SaleItem?> GetSaleItemByIdAsync(Guid saleItemId)
        {
            var sql = @"
                SELECT
                    si.sale_item_id AS SaleItemId,
                    si.sale_id AS SaleId,
                    si.product_id AS ProductId,
                    si.quantity AS Quantity,
                    si.total_price AS TotalPrice,
                    si.created_at AS CreatedAt,
                    p.product_id AS ProductId,
                    p.reference AS Reference,
                    p.name AS Name,
                    p.category_id AS CategoryId,
                    p.price AS Price,
                    p.stock_quantity AS StockQuantity,
                    p.created_at AS CreatedAt
                FROM sale_item si
                LEFT JOIN product p ON si.product_id = p.product_id
                WHERE si.sale_item_id = @Id;
            ";

            var result = await _db.QueryAsync<SaleItem, Product, SaleItem>(
                sql,
                (saleItem, product) =>
                {
                    saleItem.Product = product;
                    return saleItem;
                },
                new { Id = saleItemId },
                splitOn: "ProductId"
            );

            return result.FirstOrDefault();
        }

        public async Task<int> CreateSaleItemAsync(SaleItem saleItem)
        {
            if (saleItem.SaleItemId == Guid.Empty)
                saleItem.SaleItemId = Guid.NewGuid();

            if (saleItem.CreatedAt == default)
                saleItem.CreatedAt = DateTime.UtcNow;

            // garante total price consistente (se o consumidor não calculou)
            saleItem.TotalPrice = saleItem.TotalPrice == 0m
                ? saleItem.Quantity * (saleItem.Product?.Price ?? 0m)
                : saleItem.TotalPrice;

            var sql = @"
                INSERT INTO sale_item (sale_item_id, sale_id, product_id, quantity, total_price, created_at)
                VALUES (@SaleItemId, @SaleId, @ProductId, @Quantity, @TotalPrice, @CreatedAt);
            ";

            var affected = await _db.ExecuteAsync(sql, new
            {
                saleItem.SaleItemId,
                saleItem.SaleId,
                saleItem.ProductId,
                saleItem.Quantity,
                saleItem.TotalPrice,
                saleItem.CreatedAt
            });

            return affected;
        }

        public async Task<int> UpdateSaleItemAsync(SaleItem saleItem)
        {
            var sql = @"
                UPDATE sale_item
                SET
                    product_id = @ProductId,
                    quantity = @Quantity,
                    total_price = @TotalPrice
                WHERE sale_item_id = @SaleItemId;
            ";

            var affected = await _db.ExecuteAsync(sql, new
            {
                saleItem.ProductId,
                saleItem.Quantity,
                saleItem.TotalPrice,
                saleItem.SaleItemId
            });

            return affected;
        }

        public async Task<int> DeleteSaleItemAsync(Guid saleItemId)
        {
            var sql = "DELETE FROM sale_item WHERE sale_item_id = @Id;";
            var affected = await _db.ExecuteAsync(sql, new { Id = saleItemId });
            return affected;
        }
    }
}
