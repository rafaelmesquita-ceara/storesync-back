namespace SharedModels.Interfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(Guid employeeId);
    Task<int> CreateEmployeeAsync(Employee employee);
    Task<int> UpdateEmployeeAsync(Employee employee);
    Task<int> DeleteEmployeeAsync(Guid employeeId);
}

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(Guid employeeId);
    Task<int> CreateEmployeeAsync(Employee employee);
    Task<int> UpdateEmployeeAsync(Employee employee);
    Task<int> DeleteEmployeeAsync(Guid employeeId);
}

