using System.Data;
using Dapper;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Repositories
{
    public class CommissionRepository : ICommissionRepository
    {
        private readonly IDbConnection _db;

        private const string SelectColumns = @"
            c.commission_id AS CommissionId,
            c.employee_id AS EmployeeId,
            c.start_date AS StartDate,
            c.end_date AS EndDate,
            c.reference AS Reference,
            c.observation AS Observation,
            c.commission_rate AS CommissionRate,
            c.total_sales AS TotalSales,
            c.commission_value AS CommissionValue,
            c.created_at AS CreatedAt,
            e.employee_id AS EmployeeId,
            e.name AS Name,
            e.cpf AS Cpf,
            e.role AS Role,
            e.commission_rate AS CommissionRate,
            e.created_at AS CreatedAt";

        public CommissionRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Commission>> GetAllCommissionsAsync()
        {
            var sql = $@"
                SELECT {SelectColumns}
                FROM commission c
                JOIN employee e ON e.employee_id = c.employee_id
                ORDER BY c.start_date DESC;
            ";

            var result = await _db.QueryAsync<Commission, Employee, Commission>(
                sql,
                (commission, employee) =>
                {
                    commission.Employee = employee;
                    return commission;
                },
                splitOn: "EmployeeId"
            );

            return result;
        }

        public async Task<Commission?> GetCommissionByIdAsync(Guid commissionId)
        {
            var sql = $@"
                SELECT {SelectColumns}
                FROM commission c
                JOIN employee e ON e.employee_id = c.employee_id
                WHERE c.commission_id = @Id;
            ";

            var result = await _db.QueryAsync<Commission, Employee, Commission>(
                sql,
                (commission, employee) =>
                {
                    commission.Employee = employee;
                    return commission;
                },
                new { Id = commissionId },
                splitOn: "EmployeeId"
            );

            return result.FirstOrDefault();
        }

        public async Task<Commission?> GetOverlappingCommissionAsync(Guid employeeId, DateTime startDate, DateTime endDate)
        {
            var sql = @"
                SELECT
                    c.commission_id AS CommissionId,
                    c.employee_id AS EmployeeId,
                    c.start_date AS StartDate,
                    c.end_date AS EndDate,
                    c.reference AS Reference,
                    c.observation AS Observation,
                    c.commission_rate AS CommissionRate,
                    c.total_sales AS TotalSales,
                    c.commission_value AS CommissionValue,
                    c.created_at AS CreatedAt
                FROM commission c
                WHERE c.employee_id = @EmployeeId
                  AND c.start_date <= @EndDate
                  AND c.end_date >= @StartDate
                LIMIT 1;
            ";

            return await _db.QueryFirstOrDefaultAsync<Commission>(sql, new
            {
                EmployeeId = employeeId,
                StartDate = startDate.Date,
                EndDate = endDate.Date
            });
        }

        public async Task<int> CreateCommissionAsync(Commission commission)
        {
            if (commission.CommissionId == null || commission.CommissionId == Guid.Empty)
                commission.CommissionId = Guid.NewGuid();

            if (commission.CreatedAt == default)
                commission.CreatedAt = DateTime.UtcNow;

            var sql = @"
                INSERT INTO commission (
                    commission_id, employee_id, start_date, end_date,
                    reference, observation, commission_rate,
                    total_sales, commission_value, created_at
                )
                VALUES (
                    @CommissionId, @EmployeeId, @StartDate, @EndDate,
                    @Reference, @Observation, @CommissionRate,
                    @TotalSales, @CommissionValue, @CreatedAt
                );
            ";

            return await _db.ExecuteAsync(sql, commission);
        }

        public async Task<int> DeleteCommissionAsync(Guid commissionId)
        {
            var sql = "DELETE FROM commission WHERE commission_id = @Id;";
            return await _db.ExecuteAsync(sql, new { Id = commissionId });
        }
    }
}
