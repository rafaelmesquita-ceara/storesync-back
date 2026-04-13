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

        public async Task<PaginatedResult<SaleItem>> GetAllSaleItemsAsync(int limit = 50, int offset = 0)
        {
            var countSql = "SELECT COUNT(*) FROM sale_item;";
            var totalCount = await _db.ExecuteScalarAsync<int>(countSql);

            var sql = @"
                SELECT
                    si.sale_item_id AS SaleItemId,
                    si.sale_id AS SaleId,
                    si.product_id AS ProductId,
                    si.quantity AS Quantity,
                    si.discount AS Discount,
                    si.addition AS Addition,
                    si.total_price AS TotalPrice,
                    si.cost_price AS CostPrice,
                    si.created_at AS CreatedAt,
                    p.reference AS Reference,
                    p.product_id AS ProductId,
                    p.name AS Name,
                    p.category_id AS CategoryId,
                    p.price AS Price,
                    p.stock_quantity AS StockQuantity,
                    p.created_at AS CreatedAt
                FROM sale_item si
                LEFT JOIN product p ON si.product_id = p.product_id
                ORDER BY si.created_at DESC
                LIMIT @Limit OFFSET @Offset;
            ";

            var result = await _db.QueryAsync<SaleItem, Product, SaleItem>(
                sql,
                (saleItem, product) =>
                {
                    saleItem.Product = product;
                    return saleItem;
                },
                new { Limit = limit, Offset = offset },
                splitOn: "Reference"
            );

            return new PaginatedResult<SaleItem>
            {
                Items = result,
                TotalCount = totalCount,
                Limit = limit,
                Offset = offset
            };
        }

        public async Task<PaginatedResult<SaleItem>> GetSaleItemsBySaleIdAsync(Guid saleId, int limit = 50, int offset = 0)
        {
            var countSql = "SELECT COUNT(*) FROM sale_item WHERE sale_id = @SaleId;";
            var totalCount = await _db.ExecuteScalarAsync<int>(countSql, new { SaleId = saleId });

            var sql = @"
                SELECT
                    si.sale_item_id AS SaleItemId,
                    si.sale_id AS SaleId,
                    si.product_id AS ProductId,
                    si.quantity AS Quantity,
                    si.discount AS Discount,
                    si.addition AS Addition,
                    si.total_price AS TotalPrice,
                    si.cost_price AS CostPrice,
                    si.created_at AS CreatedAt,
                    p.reference AS Reference,
                    p.product_id AS ProductId,
                    p.name AS Name,
                    p.category_id AS CategoryId,
                    p.price AS Price,
                    p.stock_quantity AS StockQuantity,
                    p.created_at AS CreatedAt
                FROM sale_item si
                LEFT JOIN product p ON si.product_id = p.product_id
                WHERE si.sale_id = @SaleId
                ORDER BY si.created_at
                LIMIT @Limit OFFSET @Offset;
            ";

            var result = await _db.QueryAsync<SaleItem, Product, SaleItem>(
                sql,
                (saleItem, product) =>
                {
                    saleItem.Product = product;
                    return saleItem;
                },
                new { SaleId = saleId, Limit = limit, Offset = offset },
                splitOn: "Reference"
            );

            return new PaginatedResult<SaleItem>
            {
                Items = result,
                TotalCount = totalCount,
                Limit = limit,
                Offset = offset
            };
        }

        public async Task<SaleItem?> GetSaleItemByIdAsync(Guid saleItemId)
        {
            var sql = @"
                SELECT
                    si.sale_item_id AS SaleItemId,
                    si.sale_id AS SaleId,
                    si.product_id AS ProductId,
                    si.quantity AS Quantity,
                    si.discount AS Discount,
                    si.addition AS Addition,
                    si.total_price AS TotalPrice,
                    si.cost_price AS CostPrice,
                    si.created_at AS CreatedAt,
                    p.reference AS Reference,
                    p.product_id AS ProductId,
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
                splitOn: "Reference"
            );

            return result.FirstOrDefault();
        }

        public async Task<int> CreateSaleItemAsync(SaleItem saleItem)
        {
            if (saleItem.SaleItemId == Guid.Empty)
                saleItem.SaleItemId = Guid.NewGuid();

            if (saleItem.CreatedAt == default)
                saleItem.CreatedAt = BrazilDateTime.Now;

            if (saleItem.TotalPrice == 0m)
                saleItem.TotalPrice = (saleItem.Quantity * (saleItem.Product?.Price ?? 0m))
                                      - saleItem.Discount + saleItem.Addition;

            var sql = @"
                INSERT INTO sale_item (sale_item_id, sale_id, product_id, quantity, discount, addition, total_price, cost_price, created_at)
                VALUES (@SaleItemId, @SaleId, @ProductId, @Quantity, @Discount, @Addition, @TotalPrice, @CostPrice, @CreatedAt);
            ";

            var affected = await _db.ExecuteAsync(sql, new
            {
                saleItem.SaleItemId,
                saleItem.SaleId,
                saleItem.ProductId,
                saleItem.Quantity,
                saleItem.Discount,
                saleItem.Addition,
                saleItem.TotalPrice,
                saleItem.CostPrice,
                saleItem.CreatedAt
            });

            await RecalculateSaleTotalAsync(saleItem.SaleId);
            return affected;
        }

        public async Task<int> UpdateSaleItemAsync(SaleItem saleItem)
        {
            var sql = @"
                UPDATE sale_item
                SET
                    product_id = @ProductId,
                    quantity = @Quantity,
                    discount = @Discount,
                    addition = @Addition,
                    total_price = @TotalPrice
                WHERE sale_item_id = @SaleItemId;
            ";

            var affected = await _db.ExecuteAsync(sql, new
            {
                saleItem.ProductId,
                saleItem.Quantity,
                saleItem.Discount,
                saleItem.Addition,
                saleItem.TotalPrice,
                saleItem.SaleItemId
            });

            await RecalculateSaleTotalAsync(saleItem.SaleId);
            return affected;
        }

        public async Task<int> DeleteSaleItemAsync(Guid saleItemId)
        {
            var saleId = await _db.ExecuteScalarAsync<Guid?>(
                "SELECT sale_id FROM sale_item WHERE sale_item_id = @Id;",
                new { Id = saleItemId });

            var sql = "DELETE FROM sale_item WHERE sale_item_id = @Id;";
            var affected = await _db.ExecuteAsync(sql, new { Id = saleItemId });

            if (saleId.HasValue)
                await RecalculateSaleTotalAsync(saleId.Value);

            return affected;
        }

        private async Task RecalculateSaleTotalAsync(Guid saleId)
        {
            var sql = @"
                UPDATE sale
                SET total_amount = COALESCE(
                    (SELECT SUM(total_price) FROM sale_item WHERE sale_id = @Id), 0)
                    - sale.discount + sale.addition
                WHERE sale_id = @Id;
            ";
            await _db.ExecuteAsync(sql, new { Id = saleId });
        }
    }
}
