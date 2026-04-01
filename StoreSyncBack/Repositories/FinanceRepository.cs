using System.Data;
using Dapper;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Repositories
{
    public class FinanceRepository : IFinanceRepository
    {
        private readonly IDbConnection _db;

        public FinanceRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Finance>> GetAllFinanceAsync()
        {
            var sql = @"
                SELECT
                    finance_id AS FinanceId,
                    description AS Description,
                    amount AS Amount,
                    due_date AS DueDate,
                    status AS Status,
                    created_at AS CreatedAt
                FROM finance
                ORDER BY due_date DESC;
            ";
            return await _db.QueryAsync<Finance>(sql);
        }

        public async Task<Finance?> GetFinanceByIdAsync(Guid financeId)
        {
            var sql = @"
                SELECT
                    finance_id AS FinanceId,
                    description AS Description,
                    amount AS Amount,
                    due_date AS DueDate,
                    status AS Status,
                    created_at AS CreatedAt
                FROM finance
                WHERE finance_id = @Id;
            ";
            return await _db.QueryFirstOrDefaultAsync<Finance?>(sql, new { Id = financeId });
        }

        public async Task<int> CreateFinanceAsync(Finance finance)
        {
            if (finance.FinanceId == Guid.Empty)
                finance.FinanceId = Guid.NewGuid();

            if (finance.CreatedAt == default)
                finance.CreatedAt = DateTime.UtcNow;

            var sql = @"
                INSERT INTO finance (finance_id, description, amount, due_date, status, created_at)
                VALUES (@FinanceId, @Description, @Amount, @DueDate, @Status, @CreatedAt);
            ";
            var affected = await _db.ExecuteAsync(sql, finance);
            return affected;
        }

        public async Task<int> UpdateFinanceAsync(Finance finance)
        {
            var sql = @"
                UPDATE finance
                SET
                    description = @Description,
                    amount = @Amount,
                    due_date = @DueDate,
                    status = @Status
                WHERE finance_id = @FinanceId;
            ";
            var affected = await _db.ExecuteAsync(sql, new
            {
                finance.Description,
                finance.Amount,
                finance.DueDate,
                finance.Status,
                finance.FinanceId
            });
            return affected;
        }

        public async Task<int> DeleteFinanceAsync(Guid financeId)
        {
            var sql = "DELETE FROM finance WHERE finance_id = @Id;";
            var affected = await _db.ExecuteAsync(sql, new { Id = financeId });
            return affected;
        }
    }
}
