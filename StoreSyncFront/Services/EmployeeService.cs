using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Models;

namespace StoreSyncFront.Services;

public class EmployeeService(IApiService apiService) : IEmployeeService
{
    public async Task<PaginatedResult<Employee>> GetAllEmployeesAsync(int limit = 50, int offset = 0)
    {
        Response response = await apiService.GetAsync($"/api/employees?limit={limit}&offset={offset}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<PaginatedResult<Employee>>(response.Body) ?? new PaginatedResult<Employee>();

        SnackBarService.SendError("Erro ao buscar funcionários: " + response.Body);
        return new PaginatedResult<Employee> { Items = new List<Employee>() };
    }

    public async Task<Employee?> GetEmployeeByIdAsync(Guid employeeId)
    {
        Response response = await apiService.GetAsync($"/api/employees/{employeeId}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<Employee>(response.Body);

        SnackBarService.SendError("Erro ao buscar funcionário: " + response.Body);
        return null;
    }

    public async Task<int> CreateEmployeeAsync(Employee employee)
    {
        Response response = await apiService.PostAsync("/api/employees", JsonContent.Create(employee));
        if (response.IsSuccess())
            SnackBarService.SendSuccess("Funcionário cadastrado com sucesso."
            );
        else
            SnackBarService.SendError("Erro ao cadastrar funcionário: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task<int> UpdateEmployeeAsync(Employee employee)
    {
        Response response = await apiService.PutAsync($"/api/employees/{employee.EmployeeId}", JsonContent.Create(employee));
        if (response.IsSuccess())
            SnackBarService.SendSuccess("Funcionário atualizado com sucesso."
            );
        else
            SnackBarService.SendError("Erro ao atualizar funcionário: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task<int> DeleteEmployeeAsync(Guid employeeId)
    {
        Response response = await apiService.DeleteAsync($"/api/employees/{employeeId}");
        if (response.IsSuccess())
            SnackBarService.SendSuccess("Funcionário excluído com sucesso."
            );
        else
            SnackBarService.SendError("Erro ao excluir funcionário: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }
}
