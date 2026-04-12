using System.Data;
using Dapper;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Repositories
{
    public class SalePaymentRepository : ISalePaymentRepository
    {
        private readonly IDbConnection _db;

        public SalePaymentRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<SalePayment>> GetBySaleIdAsync(Guid saleId)
        {
            var sql = @"
                SELECT
                    sp.sale_payment_id   AS SalePaymentId,
                    sp.sale_id           AS SaleId,
                    sp.payment_method_id AS PaymentMethodId,
                    sp.amount            AS Amount,
                    sp.installments      AS Installments,
                    sp.surcharge_applied AS SurchargeApplied,
                    sp.surcharge_amount  AS SurchargeAmount,
                    sp.created_at        AS CreatedAt,
                    pm.name              AS Name,
                    pm.payment_method_id AS PaymentMethodId,
                    pm.type              AS Type,
                    pm.status            AS Status,
                    pm.created_at        AS CreatedAt,
                    pm.updated_at        AS UpdatedAt
                FROM sale_payment sp
                JOIN payment_method pm ON sp.payment_method_id = pm.payment_method_id
                WHERE sp.sale_id = @Id
                ORDER BY sp.created_at;
            ";

            var payments = await _db.QueryAsync<SalePayment, PaymentMethod, SalePayment>(
                sql,
                (payment, pm) =>
                {
                    payment.PaymentMethod = pm;
                    return payment;
                },
                new { Id = saleId },
                splitOn: "Name"
            );

            return payments;
        }

        public async Task<SalePayment?> GetByIdAsync(Guid salePaymentId)
        {
            var sql = @"
                SELECT
                    sp.sale_payment_id   AS SalePaymentId,
                    sp.sale_id           AS SaleId,
                    sp.payment_method_id AS PaymentMethodId,
                    sp.amount            AS Amount,
                    sp.installments      AS Installments,
                    sp.surcharge_applied AS SurchargeApplied,
                    sp.surcharge_amount  AS SurchargeAmount,
                    sp.created_at        AS CreatedAt,
                    pm.name              AS Name,
                    pm.payment_method_id AS PaymentMethodId,
                    pm.type              AS Type,
                    pm.status            AS Status,
                    pm.created_at        AS CreatedAt,
                    pm.updated_at        AS UpdatedAt
                FROM sale_payment sp
                JOIN payment_method pm ON sp.payment_method_id = pm.payment_method_id
                WHERE sp.sale_payment_id = @Id;
            ";

            var result = await _db.QueryAsync<SalePayment, PaymentMethod, SalePayment>(
                sql,
                (payment, pm) =>
                {
                    payment.PaymentMethod = pm;
                    return payment;
                },
                new { Id = salePaymentId },
                splitOn: "Name"
            );

            return result.FirstOrDefault();
        }

        public async Task<Guid> CreateAsync(SalePayment payment)
        {
            if (payment.SalePaymentId == Guid.Empty)
                payment.SalePaymentId = Guid.NewGuid();

            payment.CreatedAt = BrazilDateTime.Now;

            var sql = @"
                INSERT INTO sale_payment (sale_payment_id, sale_id, payment_method_id, amount, installments, surcharge_applied, surcharge_amount, created_at)
                VALUES (@SalePaymentId, @SaleId, @PaymentMethodId, @Amount, @Installments, @SurchargeApplied, @SurchargeAmount, @CreatedAt);
            ";

            await _db.ExecuteAsync(sql, payment);
            return payment.SalePaymentId;
        }

        public async Task<int> DeleteAsync(Guid salePaymentId)
        {
            return await _db.ExecuteAsync(
                "DELETE FROM sale_payment WHERE sale_payment_id = @Id;",
                new { Id = salePaymentId });
        }

        public async Task<decimal> GetTotalPaidBySaleIdAsync(Guid saleId)
        {
            return await _db.ExecuteScalarAsync<decimal>(
                "SELECT COALESCE(SUM(amount), 0) FROM sale_payment WHERE sale_id = @Id;",
                new { Id = saleId });
        }
    }
}
