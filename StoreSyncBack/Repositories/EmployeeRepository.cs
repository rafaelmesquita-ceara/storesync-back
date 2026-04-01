using System.Data;
using Dapper;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IDbConnection _db;

        public EmployeeRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            var sql = @"
                SELECT
                    employee_id AS EmployeeId,
                    name AS Name,
                    cpf AS Cpf,
                    role AS Role,
                    commission_rate AS CommissionRate,
                    created_at AS CreatedAt
                FROM employee
                ORDER BY name;
            ";
            return await _db.QueryAsync<Employee>(sql);
        }

        public async Task<Employee?> GetEmployeeByIdAsync(Guid employeeId)
        {
            var sql = @"
                SELECT
                    employee_id AS EmployeeId,
                    name AS Name,
                    cpf AS Cpf,
                    role AS Role,
                    commission_rate AS CommissionRate,
                    created_at AS CreatedAt
                FROM employee
                WHERE employee_id = @Id;
            ";
            return await _db.QueryFirstOrDefaultAsync<Employee?>(sql, new { Id = employeeId });
        }

        public async Task<int> CreateEmployeeAsync(Employee employee)
        {
            if (employee.EmployeeId == Guid.Empty)
                employee.EmployeeId = Guid.NewGuid();

            if (employee.CreatedAt == default)
                employee.CreatedAt = DateTime.UtcNow;

            var sql = @"
                INSERT INTO employee (employee_id, name, cpf, role, commission_rate, created_at)
                VALUES (@EmployeeId, @Name, @Cpf, @Role, @CommissionRate, @CreatedAt);
            ";
            var affected = await _db.ExecuteAsync(sql, employee);
            return affected;
        }

        public async Task<int> UpdateEmployeeAsync(Employee employee)
        {
            var sql = @"
                UPDATE employee
                SET
                    name = @Name,
                    cpf = @Cpf,
                    role = @Role,
                    commission_rate = @CommissionRate
                WHERE employee_id = @EmployeeId;
            ";
            var affected = await _db.ExecuteAsync(sql, new
            {
                employee.Name,
                employee.Cpf,
                employee.Role,
                employee.CommissionRate,
                employee.EmployeeId
            });
            return affected;
        }

        public async Task<int> DeleteEmployeeAsync(Guid employeeId)
        {
            var sql = "DELETE FROM employee WHERE employee_id = @Id;";
            var affected = await _db.ExecuteAsync(sql, new { Id = employeeId });
            return affected;
        }
    }
}
