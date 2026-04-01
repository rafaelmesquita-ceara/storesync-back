using System.Data;
using Dapper;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Repositories
{
    public class CommissionRepository : ICommissionRepository
    {
        private readonly IDbConnection _db;

        public CommissionRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Commission>> GetAllCommissionsAsync()
        {
            var sql = @"
                SELECT
                    c.commission_id AS CommissionId,
                    c.employee_id AS EmployeeId,
                    c.month AS Month,
                    c.total_sales AS TotalSales,
                    c.commission_value AS CommissionValue,
                    c.created_at AS CreatedAt,
                    e.employee_id AS EmployeeId,
                    e.name AS Name,
                    e.cpf AS Cpf,
                    e.role AS Role,
                    e.commission_rate AS CommissionRate,
                    e.created_at AS CreatedAt
                FROM commission c
                JOIN employee e ON e.employee_id = c.employee_id
                ORDER BY c.month DESC;
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
            var sql = @"
                SELECT
                    c.commission_id AS CommissionId,
                    c.employee_id AS EmployeeId,
                    c.month AS Month,
                    c.total_sales AS TotalSales,
                    c.commission_value AS CommissionValue,
                    c.created_at AS CreatedAt,
                    e.employee_id AS EmployeeId,
                    e.name AS Name,
                    e.cpf AS Cpf,
                    e.role AS Role,
                    e.commission_rate AS CommissionRate,
                    e.created_at AS CreatedAt
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

        public async Task<int> CreateCommissionAsync(Commission commission)
        {
            if (commission.CommissionId == null || commission.CommissionId == Guid.Empty)
                commission.CommissionId = Guid.NewGuid();

            if (commission.CreatedAt == default)
                commission.CreatedAt = DateTime.UtcNow;

            var sql = @"
                INSERT INTO commission (commission_id, employee_id, month, total_sales, commission_value, created_at)
                VALUES (@CommissionId, @EmployeeId, @Month, @TotalSales, @CommissionValue, @CreatedAt);
            ";

            var affected = await _db.ExecuteAsync(sql, commission);
            return affected;
        }

        public async Task<int> UpdateCommissionAsync(Commission commission)
        {
            var sql = @"
                UPDATE commission
                SET
                    employee_id = @EmployeeId,
                    month = @Month,
                    total_sales = @TotalSales,
                    commission_value = @CommissionValue
                WHERE commission_id = @CommissionId;
            ";

            var affected = await _db.ExecuteAsync(sql, commission);
            return affected;
        }

        public async Task<int> DeleteCommissionAsync(Guid commissionId)
        {
            var sql = "DELETE FROM commission WHERE commission_id = @Id;";
            var affected = await _db.ExecuteAsync(sql, new { Id = commissionId });
            return affected;
        }
    }
}
