using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeService(IEmployeeRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return _repo.GetAllEmployeesAsync();
        }

        public Task<Employee?> GetEmployeeByIdAsync(Guid employeeId)
        {
            return _repo.GetEmployeeByIdAsync(employeeId);
        }

        public async Task<int> CreateEmployeeAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (string.IsNullOrWhiteSpace(employee.Name))
                throw new ArgumentException("Name é obrigatório.", nameof(employee.Name));

            // CPF: validação básica (só check null/empty). Se quiser validação mais forte, implemente algoritmo.
            if (string.IsNullOrWhiteSpace(employee.Cpf))
                throw new ArgumentException("Cpf é obrigatório.", nameof(employee.Cpf));

            if (employee.CommissionRate < 0)
                throw new ArgumentException("CommissionRate não pode ser negativo.", nameof(employee.CommissionRate));

            if (employee.EmployeeId == Guid.Empty)
                employee.EmployeeId = Guid.NewGuid();

            if (employee.CreatedAt == default)
                employee.CreatedAt = DateTime.UtcNow;

            return await _repo.CreateEmployeeAsync(employee);
        }

        public async Task<int> UpdateEmployeeAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (employee.EmployeeId == Guid.Empty)
                throw new ArgumentException("EmployeeId inválido.", nameof(employee.EmployeeId));

            if (string.IsNullOrWhiteSpace(employee.Name))
                throw new ArgumentException("Name é obrigatório.", nameof(employee.Name));

            if (employee.CommissionRate < 0)
                throw new ArgumentException("CommissionRate não pode ser negativo.", nameof(employee.CommissionRate));

            return await _repo.UpdateEmployeeAsync(employee);
        }

        public Task<int> DeleteEmployeeAsync(Guid employeeId)
        {
            if (employeeId == Guid.Empty)
                throw new ArgumentException("EmployeeId inválido.", nameof(employeeId));

            return _repo.DeleteEmployeeAsync(employeeId);
        }
    }
}
