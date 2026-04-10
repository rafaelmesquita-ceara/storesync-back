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
                    s.referencia AS Referencia,
                    s.employee_id AS EmployeeId,
                    s.discount AS Discount,
                    s.addition AS Addition,
                    s.total_amount AS TotalAmount,
                    s.status AS Status,
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
                    s.referencia AS Referencia,
                    s.employee_id AS EmployeeId,
                    s.discount AS Discount,
                    s.addition AS Addition,
                    s.total_amount AS TotalAmount,
                    s.status AS Status,
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
                    si.discount AS Discount,
                    si.addition AS Addition,
                    si.total_price AS TotalPrice,
                    si.created_at AS CreatedAt,
                    p.product_id AS ProductId,
                    p.reference AS Reference,
                    p.name AS Name,
                    p.price AS Price,
                    p.stock_quantity AS StockQuantity
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

            sale.Status = SaleStatus.Aberta;

            const string insertSale = @"
                INSERT INTO sale (sale_id, employee_id, discount, addition, total_amount, status, sale_date, created_at)
                VALUES (@SaleId, @EmployeeId, @Discount, @Addition, @TotalAmount, @Status, @SaleDate, @CreatedAt)
                RETURNING referencia;
            ";

            sale.Referencia = await _db.ExecuteScalarAsync<string>(insertSale, sale);
            return sale.SaleId;
        }

        public async Task<int> UpdateSaleAsync(Sale sale)
        {
            var sql = @"
                UPDATE sale
                SET
                    employee_id = @EmployeeId,
                    discount = @Discount,
                    addition = @Addition,
                    total_amount = @TotalAmount
                WHERE sale_id = @SaleId AND status = @StatusAberta;
            ";

            return await _db.ExecuteAsync(sql, new
            {
                sale.EmployeeId,
                sale.Discount,
                sale.Addition,
                sale.TotalAmount,
                sale.SaleId,
                StatusAberta = SaleStatus.Aberta
            });
        }

        public async Task<int> FinalizeSaleAsync(Guid saleId)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            using var transaction = _db.BeginTransaction();

            try
            {
                var sale = (await _db.QueryAsync<Sale>(
                    "SELECT sale_id AS SaleId, status AS Status FROM sale WHERE sale_id = @Id;",
                    new { Id = saleId }, transaction)).FirstOrDefault();

                if (sale == null)
                    throw new ArgumentException("Venda não encontrada.");
                if (sale.Status != SaleStatus.Aberta)
                    throw new InvalidOperationException("Apenas vendas em aberto podem ser finalizadas.");

                var items = await _db.QueryAsync<SaleItem>(
                    @"SELECT sale_item_id AS SaleItemId, product_id AS ProductId, quantity AS Quantity
                      FROM sale_item WHERE sale_id = @Id;",
                    new { Id = saleId }, transaction);

                var itemList = items.ToList();
                if (itemList.Count == 0)
                    throw new InvalidOperationException("A venda deve conter pelo menos um item para ser finalizada.");

                foreach (var item in itemList)
                {
                    var stock = await _db.ExecuteScalarAsync<int>(
                        "SELECT stock_quantity FROM product WHERE product_id = @Id;",
                        new { Id = item.ProductId }, transaction);

                    if (item.Quantity > stock)
                        throw new InvalidOperationException($"Estoque insuficiente para o produto {item.ProductId}.");

                    await _db.ExecuteAsync(
                        "UPDATE product SET stock_quantity = stock_quantity - @Qty WHERE product_id = @Id;",
                        new { Qty = item.Quantity, Id = item.ProductId }, transaction);
                }

                var affected = await _db.ExecuteAsync(
                    "UPDATE sale SET status = @Status WHERE sale_id = @Id;",
                    new { Status = SaleStatus.Finalizada, Id = saleId }, transaction);

                transaction.Commit();
                return affected;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> CancelSaleAsync(Guid saleId)
        {
            if (_db.State != ConnectionState.Open)
                _db.Open();

            using var transaction = _db.BeginTransaction();

            try
            {
                var sale = (await _db.QueryAsync<Sale>(
                    "SELECT sale_id AS SaleId, status AS Status FROM sale WHERE sale_id = @Id;",
                    new { Id = saleId }, transaction)).FirstOrDefault();

                if (sale == null)
                    throw new ArgumentException("Venda não encontrada.");
                if (sale.Status == SaleStatus.Cancelada)
                    throw new InvalidOperationException("Esta venda já está cancelada.");

                if (sale.Status == SaleStatus.Finalizada)
                {
                    var items = await _db.QueryAsync<SaleItem>(
                        @"SELECT sale_item_id AS SaleItemId, product_id AS ProductId, quantity AS Quantity
                          FROM sale_item WHERE sale_id = @Id;",
                        new { Id = saleId }, transaction);

                    foreach (var item in items)
                    {
                        await _db.ExecuteAsync(
                            "UPDATE product SET stock_quantity = stock_quantity + @Qty WHERE product_id = @Id;",
                            new { Qty = item.Quantity, Id = item.ProductId }, transaction);
                    }
                }

                var affected = await _db.ExecuteAsync(
                    "UPDATE sale SET status = @Status WHERE sale_id = @Id;",
                    new { Status = SaleStatus.Cancelada, Id = saleId }, transaction);

                transaction.Commit();
                return affected;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
