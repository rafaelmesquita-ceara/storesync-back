using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncFront.ViewModels;

public partial class EmployeesViewModel : ObservableValidator
{
    [ObservableProperty]
    private string _searchBarField = string.Empty;

    private readonly IEmployeeService _employeeService;

    public ObservableCollection<Employee> Employees { get; } = new();
    private List<Employee>? _allEmployees;

    // Cargos disponíveis — populados com defaults + carregados do banco
    public ObservableCollection<string> Roles { get; }

    [ObservableProperty] private bool _isEdit;
    [ObservableProperty] private Guid _employeeId = Guid.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    private string _name = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "O campo CPF é obrigatório.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve ter exatamente 11 dígitos numéricos.")]
    private string _cpf = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "O campo Taxa de Comissão é obrigatório.")]
    [RegularExpression(@"^\d+([,.]\d{1,2})?$", ErrorMessage = "Informe um valor numérico (ex: 5 ou 5,50).")]
    private string _commissionRate = string.Empty;

    [ObservableProperty] private string? _selectedRole;

    public IRelayCommand ToggleEditCommand { get; }

    public EmployeesViewModel(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
        ToggleEditCommand = new RelayCommand(() => IsEdit = !IsEdit);
        Roles = new ObservableCollection<string> { "admin", "vendedor", "gerente", "caixa" };
    }

    public async Task LoadDataAsync()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        Employees.Clear();
        foreach (var e in employees)
        {
            Employees.Add(e);
            if (!string.IsNullOrWhiteSpace(e.Role) && !Roles.Contains(e.Role))
                Roles.Add(e.Role);
        }
        _allEmployees = Employees.ToList();
    }

    [RelayCommand]
    private async Task AddEmployee()
    {
        ClearErrors();
        ValidateAllProperties();
        if (HasErrors) return;

        decimal.TryParse(CommissionRate.Replace(',', '.'),
            NumberStyles.Any, CultureInfo.InvariantCulture, out var rate);

        var employee = new Employee
        {
            Name = Name,
            Cpf = Cpf,
            Role = SelectedRole,
            CommissionRate = rate
        };

        if (EmployeeId != Guid.Empty)
        {
            employee.EmployeeId = EmployeeId;
            await _employeeService.UpdateEmployeeAsync(employee);
            ClearForm();
            await LoadDataAsync();
            return;
        }

        await _employeeService.CreateEmployeeAsync(employee);
        ClearForm();
        await LoadDataAsync();
    }

    [RelayCommand]
    public void OpenEdit(Guid employeeId)
    {
        ClearErrors();
        var emp = Employees.FirstOrDefault(e => e.EmployeeId == employeeId);
        if (emp == null) return;

        EmployeeId = emp.EmployeeId;
        Name = emp.Name ?? string.Empty;
        Cpf = emp.Cpf ?? string.Empty;
        CommissionRate = emp.CommissionRate.ToString(CultureInfo.CurrentCulture);
        SelectedRole = emp.Role;
        IsEdit = true;
    }

    [RelayCommand]
    public async void Delete(Guid employeeId)
    {
        await _employeeService.DeleteEmployeeAsync(employeeId);
        await LoadDataAsync();
    }

    [RelayCommand]
    public void ClearForm()
    {
        ClearErrors();
        IsEdit = false;
        EmployeeId = Guid.Empty;
        Name = string.Empty;
        Cpf = string.Empty;
        CommissionRate = string.Empty;
        SelectedRole = null;
    }

    [RelayCommand]
    public async void Refresh()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    public void Search()
    {
        if (_allEmployees == null) _allEmployees = Employees.ToList();

        var query = (_searchBarField ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            Employees.Clear();
            foreach (var e in _allEmployees) Employees.Add(e);
            return;
        }

        var tokens = Regex.Split(query, @"\s+")
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(Normalize)
            .ToArray();

        var filtered = _allEmployees.Where(emp =>
        {
            var combined = new StringBuilder();
            combined.Append(emp.Name ?? "").Append(' ');
            combined.Append(emp.Cpf ?? "").Append(' ');
            combined.Append(emp.Role ?? "").Append(' ');
            combined.Append(emp.CommissionRate.ToString(CultureInfo.InvariantCulture)).Append(' ');
            combined.Append(emp.EmployeeId.ToString());
            var norm = Normalize(combined.ToString());
            return tokens.All(t => norm.Contains(t));
        }).ToList();

        Employees.Clear();
        foreach (var e in filtered) Employees.Add(e);
    }

    public void AddRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role)) return;
        if (!Roles.Contains(role)) Roles.Add(role);
        SelectedRole = role;
    }

    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}
