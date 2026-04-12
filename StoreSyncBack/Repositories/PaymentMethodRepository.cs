using System.Data;
using Dapper;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Repositories
{
    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly IDbConnection _db;

        public PaymentMethodRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<PaymentMethod>> GetAllAsync()
        {
            var sqlMethods = @"
                SELECT
                    payment_method_id AS PaymentMethodId,
                    name              AS Name,
                    type              AS Type,
                    status            AS Status,
                    created_at        AS CreatedAt,
                    updated_at        AS UpdatedAt
                FROM payment_method
                WHERE status = 1
                ORDER BY name;
            ";

            var methods = (await _db.QueryAsync<PaymentMethod>(sqlMethods)).ToList();

            if (methods.Count == 0)
                return methods;

            var methodIds = methods.Select(m => m.PaymentMethodId).ToList();

            var sqlRates = @"
                SELECT
                    rate_id           AS RateId,
                    payment_method_id AS PaymentMethodId,
                    installments      AS Installments,
                    rate_percentage   AS RatePercentage,
                    created_at        AS CreatedAt
                FROM payment_method_rate
                WHERE payment_method_id = ANY(@Ids)
                ORDER BY installments;
            ";

            var rates = (await _db.QueryAsync<PaymentMethodRate>(sqlRates, new { Ids = methodIds })).ToList();

            foreach (var m in methods)
                m.Rates = rates.Where(r => r.PaymentMethodId == m.PaymentMethodId).ToList();

            return methods;
        }

        public async Task<PaymentMethod?> GetByIdAsync(Guid id)
        {
            var sql = @"
                SELECT
                    payment_method_id AS PaymentMethodId,
                    name              AS Name,
                    type              AS Type,
                    status            AS Status,
                    created_at        AS CreatedAt,
                    updated_at        AS UpdatedAt
                FROM payment_method
                WHERE payment_method_id = @Id;
            ";

            var pm = await _db.QueryFirstOrDefaultAsync<PaymentMethod>(sql, new { Id = id });
            if (pm == null)
                return null;

            pm.Rates = (await GetRatesByMethodIdAsync(id)).ToList();
            return pm;
        }

        public async Task<Guid> CreateAsync(PaymentMethod pm)
        {
            if (pm.PaymentMethodId == Guid.Empty)
                pm.PaymentMethodId = Guid.NewGuid();

            pm.CreatedAt = BrazilDateTime.Now;
            pm.UpdatedAt = BrazilDateTime.Now;

            var sql = @"
                INSERT INTO payment_method (payment_method_id, name, type, status, created_at, updated_at)
                VALUES (@PaymentMethodId, @Name, @Type, @Status, @CreatedAt, @UpdatedAt);
            ";

            await _db.ExecuteAsync(sql, pm);
            return pm.PaymentMethodId;
        }

        public async Task<int> UpdateAsync(PaymentMethod pm)
        {
            pm.UpdatedAt = BrazilDateTime.Now;

            var sql = @"
                UPDATE payment_method
                SET name       = @Name,
                    type       = @Type,
                    status     = @Status,
                    updated_at = @UpdatedAt
                WHERE payment_method_id = @PaymentMethodId;
            ";

            return await _db.ExecuteAsync(sql, pm);
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            var sql = @"
                UPDATE payment_method
                SET status = 0, updated_at = @UpdatedAt
                WHERE payment_method_id = @Id;
            ";

            return await _db.ExecuteAsync(sql, new { Id = id, UpdatedAt = BrazilDateTime.Now });
        }

        public async Task<bool> IsUsedInSalesAsync(Guid id)
        {
            var sql = "SELECT COUNT(1) FROM sale_payment WHERE payment_method_id = @Id;";
            var count = await _db.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<IEnumerable<PaymentMethodRate>> GetRatesByMethodIdAsync(Guid methodId)
        {
            var sql = @"
                SELECT
                    rate_id           AS RateId,
                    payment_method_id AS PaymentMethodId,
                    installments      AS Installments,
                    rate_percentage   AS RatePercentage,
                    created_at        AS CreatedAt
                FROM payment_method_rate
                WHERE payment_method_id = @Id
                ORDER BY installments;
            ";

            return await _db.QueryAsync<PaymentMethodRate>(sql, new { Id = methodId });
        }

        public async Task<Guid> AddRateAsync(PaymentMethodRate rate)
        {
            if (rate.RateId == Guid.Empty)
                rate.RateId = Guid.NewGuid();

            rate.CreatedAt = BrazilDateTime.Now;

            var sql = @"
                INSERT INTO payment_method_rate (rate_id, payment_method_id, installments, rate_percentage, created_at)
                VALUES (@RateId, @PaymentMethodId, @Installments, @RatePercentage, @CreatedAt);
            ";

            await _db.ExecuteAsync(sql, rate);
            return rate.RateId;
        }

        public async Task<int> DeleteRateAsync(Guid rateId)
        {
            return await _db.ExecuteAsync(
                "DELETE FROM payment_method_rate WHERE rate_id = @Id;",
                new { Id = rateId });
        }
    }
}
