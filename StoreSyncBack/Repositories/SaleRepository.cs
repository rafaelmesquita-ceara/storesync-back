using System.Data;
using Dapper;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly IDbConnection _db;

        public SaleRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Sale>> GetAllSalesAsync()
        {
            var sql = @"
                SELECT
                    s.sale_id AS SaleId,
                    s.employee_id AS EmployeeId,
                    s.total_amount AS TotalAmount,
                    s.sale_date AS SaleDate,
                    s.created_at AS CreatedAt,
                    e.employee_id AS EmployeeId,
                    e.name AS Name,
                    e.cpf AS Cpf,
                    e.role AS Role,
                    e.commission_rate AS CommissionRate,
                    e.created_at AS CreatedAt
                FROM sale s
                JOIN employee e ON s.employee_id = e.employee_id
                ORDER BY s.sale_date DESC;
            ";

            var sales = await _db.QueryAsync<Sale, Employee, Sale>(
                sql,
                (sale, employee) =>
                {
                    sale.Employee = employee;
                    return sale;
                },
                splitOn: "EmployeeId"
            );

            return sales;
        }

        public async Task<Sale?> GetSaleByIdAsync(Guid saleId)
        {
            var sqlSale = @"
                SELECT
                    s.sale_id AS SaleId,
                    s.employee_id AS EmployeeId,
                    s.total_amount AS TotalAmount,
                    s.sale_date AS SaleDate,
                    s.created_at AS CreatedAt,
                    e.employee_id AS EmployeeId,
                    e.name AS Name,
                    e.cpf AS Cpf,
                    e.role AS Role,
                    e.commission_rate AS CommissionRate,
                    e.created_at AS CreatedAt
                FROM sale s
                JOIN employee e ON s.employee_id = e.employee_id
                WHERE s.sale_id = @Id;
            ";

            var result = await _db.QueryAsync<Sale, Employee, Sale>(
                sqlSale,
                (sale, employee) =>
                {
                    sale.Employee = employee;
                    return sale;
                },
                new { Id = saleId },
                splitOn: "EmployeeId"
            );

            var sale = result.FirstOrDefault();
            if (sale == null)
                return null;

            var sqlItems = @"
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
                    p.price AS Price
                FROM sale_item si
                JOIN product p ON si.product_id = p.product_id
                WHERE si.sale_id = @Id;
            ";

            var items = await _db.QueryAsync<SaleItem, Product, SaleItem>(
                sqlItems,
                (item, product) =>
                {
                    item.Product = product;
                    return item;
                },
                new { Id = saleId },
                splitOn: "ProductId"
            );

            sale.Items = items.ToList();
            return sale;
        }

        public async Task<Guid> CreateSaleAsync(Sale sale)
        {
            if (sale.SaleId == Guid.Empty)
                sale.SaleId = Guid.NewGuid();

            if (sale.CreatedAt == default)
                sale.CreatedAt = DateTime.UtcNow;

            if (sale.SaleDate == default)
                sale.SaleDate = DateTime.UtcNow;

            using var transaction = _db.BeginTransaction();

            try
            {
                const string insertSale = @"
                    INSERT INTO sale (sale_id, employee_id, total_amount, sale_date, created_at)
                    VALUES (@SaleId, @EmployeeId, @TotalAmount, @SaleDate, @CreatedAt);
                ";

                await _db.ExecuteAsync(insertSale, sale, transaction);

                if (sale.Items != null && sale.Items.Count > 0)
                {
                    const string insertItem = @"
                        INSERT INTO sale_item (sale_item_id, sale_id, product_id, quantity, total_price, created_at)
                        VALUES (@SaleItemId, @SaleId, @ProductId, @Quantity, @TotalPrice, @CreatedAt);
                    ";

                    foreach (var item in sale.Items)
                    {
                        if (item.SaleItemId == Guid.Empty)
                            item.SaleItemId = Guid.NewGuid();

                        item.SaleId = sale.SaleId;

                        if (item.CreatedAt == default)
                            item.CreatedAt = DateTime.UtcNow;

                        item.TotalPrice = item.TotalPrice == 0
                            ? item.Quantity * (item.Product?.Price ?? 0)
                            : item.TotalPrice;

                        await _db.ExecuteAsync(insertItem, item, transaction);
                    }
                }

                transaction.Commit();
                return sale.SaleId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> UpdateSaleAsync(Sale sale)
        {
            var sql = @"
                UPDATE sale
                SET
                    employee_id = @EmployeeId,
                    total_amount = @TotalAmount,
                    sale_date = @SaleDate
                WHERE sale_id = @SaleId;
            ";

            var affected = await _db.ExecuteAsync(sql, sale);
            return affected;
        }

        public async Task<int> DeleteSaleAsync(Guid saleId)
        {
            var sql = "DELETE FROM sale WHERE sale_id = @Id;";
            var affected = await _db.ExecuteAsync(sql, new { Id = saleId });
            return affected;
        }
    }
}
