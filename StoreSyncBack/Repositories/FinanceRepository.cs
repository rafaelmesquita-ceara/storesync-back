using System.Data;
using Dapper;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Repositories
{
    public class FinanceRepository : IFinanceRepository
    {
        private readonly IDbConnection _db;

        private const string SelectColumns = @"
            finance_id      AS FinanceId,
            reference       AS Reference,
            description     AS Description,
            amount          AS Amount,
            due_date        AS DueDate,
            status          AS Status,
            type            AS Type,
            title_type      AS TitleType,
            settled_amount  AS SettledAmount,
            settled_at      AS SettledAt,
            settled_note    AS SettledNote,
            parent_id       AS ParentId,
            created_at      AS CreatedAt";

        public FinanceRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<PaginatedResult<Finance>> GetAllFinanceAsync(int limit = 50, int offset = 0)
        {
            var countSql = "SELECT COUNT(*) FROM finance;";
            var totalCount = await _db.ExecuteScalarAsync<int>(countSql);

            var sql = $@"
                SELECT {SelectColumns}
                FROM finance
                ORDER BY due_date DESC
                LIMIT @Limit OFFSET @Offset;";
            var result = await _db.QueryAsync<Finance>(sql, new { Limit = limit, Offset = offset });

            return new PaginatedResult<Finance>
            {
                Items = result,
                TotalCount = totalCount,
                Limit = limit,
                Offset = offset
            };
        }

        public async Task<PaginatedResult<Finance>> GetAllByTypeAsync(int type, int limit = 50, int offset = 0)
        {
            var countSql = "SELECT COUNT(*) FROM finance WHERE type = @Type;";
            var totalCount = await _db.ExecuteScalarAsync<int>(countSql, new { Type = type });

            var sql = $@"
                SELECT {SelectColumns}
                FROM finance
                WHERE type = @Type
                ORDER BY due_date DESC
                LIMIT @Limit OFFSET @Offset;";
            var result = await _db.QueryAsync<Finance>(sql, new { Type = type, Limit = limit, Offset = offset });

            return new PaginatedResult<Finance>
            {
                Items = result,
                TotalCount = totalCount,
                Limit = limit,
                Offset = offset
            };
        }

        public async Task<Finance?> GetFinanceByIdAsync(Guid financeId)
        {
            var sql = $@"
                SELECT {SelectColumns}
                FROM finance
                WHERE finance_id = @Id;";
            return await _db.QueryFirstOrDefaultAsync<Finance?>(sql, new { Id = financeId });
        }

        public async Task<Finance?> GetResidualByParentIdAsync(Guid parentId)
        {
            var sql = $@"
                SELECT {SelectColumns}
                FROM finance
                WHERE parent_id = @ParentId
                  AND title_type = {FinanceTitleType.Residual}
                LIMIT 1;";
            return await _db.QueryFirstOrDefaultAsync<Finance?>(sql, new { ParentId = parentId });
        }

        public async Task<int> CreateFinanceAsync(Finance finance)
        {
            if (finance.FinanceId == Guid.Empty)
                finance.FinanceId = Guid.NewGuid();

            if (finance.CreatedAt == default)
                finance.CreatedAt = BrazilDateTime.Now;

            var sql = @"
                INSERT INTO finance (
                    finance_id, reference, description, amount, due_date,
                    status, type, title_type, settled_amount, settled_at,
                    settled_note, parent_id, created_at
                ) VALUES (
                    @FinanceId, @Reference, @Description, @Amount, @DueDate,
                    @Status, @Type, @TitleType, @SettledAmount, @SettledAt,
                    @SettledNote, @ParentId, @CreatedAt
                );";
            return await _db.ExecuteAsync(sql, finance);
        }

        public async Task<int> UpdateFinanceAsync(Finance finance)
        {
            var sql = @"
                UPDATE finance
                SET
                    reference    = @Reference,
                    description  = @Description,
                    amount       = @Amount,
                    due_date     = @DueDate,
                    type         = @Type
                WHERE finance_id = @FinanceId;";
            return await _db.ExecuteAsync(sql, new
            {
                finance.Reference,
                finance.Description,
                finance.Amount,
                finance.DueDate,
                finance.Type,
                finance.FinanceId
            });
        }

        public async Task<int> DeleteFinanceAsync(Guid financeId)
        {
            var sql = "DELETE FROM finance WHERE finance_id = @Id;";
            return await _db.ExecuteAsync(sql, new { Id = financeId });
        }

        public async Task<int> SettleAsync(Guid financeId, decimal settledAmount, DateTime settledAt, string? settledNote, int status)
        {
            var sql = @"
                UPDATE finance
                SET
                    status         = @Status,
                    settled_amount = @SettledAmount,
                    settled_at     = @SettledAt,
                    settled_note   = @SettledNote
                WHERE finance_id = @FinanceId;";
            return await _db.ExecuteAsync(sql, new
            {
                Status = status,
                SettledAmount = settledAmount,
                SettledAt = settledAt,
                SettledNote = settledNote,
                FinanceId = financeId
            });
        }

        public async Task<int> CancelSettlementAsync(Guid financeId)
        {
            var sql = @"
                UPDATE finance
                SET
                    status         = @Status,
                    settled_amount = NULL,
                    settled_at     = NULL,
                    settled_note   = NULL
                WHERE finance_id = @FinanceId;";
            return await _db.ExecuteAsync(sql, new
            {
                Status = FinanceStatus.Aberto,
                FinanceId = financeId
            });
        }
    }
}
