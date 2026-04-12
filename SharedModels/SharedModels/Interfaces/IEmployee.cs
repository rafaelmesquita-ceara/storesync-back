namespace SharedModels.Interfaces;

public interface IEmployeeRepository
{
    Task<PaginatedResult<Employee>> GetAllEmployeesAsync(int limit = 50, int offset = 0);
    Task<Employee?> GetEmployeeByIdAsync(Guid employeeId);
    Task<int> CreateEmployeeAsync(Employee employee);
    Task<int> UpdateEmployeeAsync(Employee employee);
    Task<int> DeleteEmployeeAsync(Guid employeeId);
}

public interface IEmployeeService
{
    Task<PaginatedResult<Employee>> GetAllEmployeesAsync(int limit = 50, int offset = 0);
    Task<Employee?> GetEmployeeByIdAsync(Guid employeeId);
    Task<int> CreateEmployeeAsync(Employee employee);
    Task<int> UpdateEmployeeAsync(Employee employee);
    Task<int> DeleteEmployeeAsync(Guid employeeId);
}

