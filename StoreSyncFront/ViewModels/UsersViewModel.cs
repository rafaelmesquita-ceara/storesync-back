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
using StoreSyncFront.Services;

namespace StoreSyncFront.ViewModels;

public partial class UserRowViewModel : ObservableObject
{
    public User Model { get; }

    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _draftLogin = string.Empty;

    public Guid UserId => Model.UserId;
    public string? Login => Model.Login;
    public Employee? Employee => Model.Employee;
    public Guid? EmployeeId => Model.EmployeeId;

    public UserRowViewModel(User model)
    {
        Model = model;
        _draftLogin = model.Login ?? string.Empty;
    }

    public void BeginEdit()
    {
        DraftLogin = Model.Login ?? string.Empty;
        IsEditing = true;
    }

    public void CancelEdit()
    {
        DraftLogin = Model.Login ?? string.Empty;
        IsEditing = false;
    }
}

public partial class UsersViewModel : ObservableValidator
{
    [ObservableProperty]
    private string _searchBarField = string.Empty;

    private readonly IAuthService _authService;
    private readonly IEmployeeService _employeeService;

    public ObservableCollection<UserRowViewModel> Users { get; } = new();
    public ObservableCollection<Employee> AvailableEmployees { get; } = new();
    private List<UserRowViewModel>? _allUsers;

    // usado apenas para o painel de criação
    [ObservableProperty] private bool _isEdit;
    [ObservableProperty] private Employee? _selectedEmployee;

    [ObservableProperty]
    [Required(ErrorMessage = "O login é obrigatório.")]
    [MinLength(3, ErrorMessage = "O login deve ter pelo menos 3 caracteres.")]
    private string _login = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    public IRelayCommand ToggleEditCommand { get; }

    public UsersViewModel(IAuthService authService, IEmployeeService employeeService)
    {
        _authService = authService;
        _employeeService = employeeService;
        ToggleEditCommand = new RelayCommand(ToggleEdit);
    }

    public async Task LoadDataAsync()
    {
        var usersTask = _authService.GetAllUsersAsync();
        var employeesTask = _employeeService.GetAllEmployeesAsync();
        await Task.WhenAll(usersTask, employeesTask);

        var employees = (await employeesTask).ToList();
        var employeeMap = employees.ToDictionary(e => e.EmployeeId);

        var userList = (await usersTask).ToList();
        foreach (var u in userList)
        {
            if (u.EmployeeId.HasValue && employeeMap.TryGetValue(u.EmployeeId.Value, out var emp))
                u.Employee = emp;
        }

        Users.Clear();
        foreach (var u in userList)
            Users.Add(new UserRowViewModel(u));

        var linkedIds = new HashSet<Guid>(userList
            .Select(u => u.EmployeeId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value));
        AvailableEmployees.Clear();
        foreach (var e in employees.Where(e => !linkedIds.Contains(e.EmployeeId)))
            AvailableEmployees.Add(e);

        _allUsers = Users.ToList();
    }

    // --- Criação de usuário ---

    [RelayCommand]
    private async Task AddUser()
    {
        ClearErrors();
        ValidateAllProperties();
        if (HasErrors) return;

        if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
        {
            SnackBarService.Send("A senha deve ter pelo menos 6 caracteres.");
            return;
        }

        if (Password != ConfirmPassword)
        {
            SnackBarService.Send("A senha e a confirmação não coincidem.");
            return;
        }

        if (SelectedEmployee == null)
        {
            SnackBarService.Send("Selecione um funcionário.");
            return;
        }

        var user = new User
        {
            Login = Login,
            Password = Password,
            EmployeeId = SelectedEmployee.EmployeeId
        };

        var code = await _authService.CreateUserAsync(user);
        if (code == 0)
        {
            ClearForm();
            await LoadDataAsync();
        }
    }

    // --- Edição inline de login ---

    public void BeginLoginEdit(Guid userId)
    {
        foreach (var row in Users)
        {
            if (row.UserId == userId)
                row.BeginEdit();
            else
                row.CancelEdit();
        }
    }

    public async Task CommitLoginEdit(UserRowViewModel row)
    {
        var login = row.DraftLogin.Trim();
        if (string.IsNullOrWhiteSpace(login) || login.Length < 3)
        {
            SnackBarService.Send("O login deve ter pelo menos 3 caracteres.");
            return;
        }

        var update = new User
        {
            UserId = row.UserId,
            Login = login,
            EmployeeId = row.EmployeeId,
            Password = null
        };

        var code = await _authService.UpdateUserAsync(update);
        if (code == 0)
        {
            row.CancelEdit();
            await LoadDataAsync();
        }
    }

    public void CancelLoginEdit(UserRowViewModel row) => row.CancelEdit();

    // --- Excluir ---

    [RelayCommand]
    public async Task Delete(Guid userId)
    {
        var logged = _authService.GetLoggedUser();
        if (logged?.UserId == userId)
        {
            SnackBarService.Send("Você não pode excluir o usuário com o qual está logado.");
            return;
        }

        await _authService.DeleteUserAsync(userId);
        await LoadDataAsync();
    }

    // --- Alterar senha ---

    public async Task ChangePasswordAsync(Guid userId, string newPassword)
    {
        var row = Users.FirstOrDefault(x => x.UserId == userId)
                  ?? _allUsers?.FirstOrDefault(x => x.UserId == userId);
        if (row == null)
        {
            SnackBarService.Send("Usuário não encontrado na lista.");
            return;
        }

        var user = new User
        {
            UserId = userId,
            Login = row.Login,
            EmployeeId = row.EmployeeId,
            Password = newPassword
        };

        var code = await _authService.UpdateUserAsync(user);
        if (code == 0)
            await LoadDataAsync();
    }

    // --- Formulário de criação ---

    [RelayCommand]
    public void ClearForm()
    {
        ClearErrors();
        IsEdit = false;
        SelectedEmployee = null;
        Login = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
    }

    [RelayCommand]
    public async Task Refresh()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    public void Search()
    {
        if (_allUsers == null) _allUsers = Users.ToList();

        var query = (SearchBarField ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            Users.Clear();
            foreach (var u in _allUsers) Users.Add(u);
            return;
        }

        var tokens = Regex.Split(query, @"\s+")
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(Normalize)
            .ToArray();

        var filtered = _allUsers.Where(row =>
        {
            var combined = new StringBuilder();
            combined.Append(row.Employee?.Name ?? "").Append(' ');
            combined.Append(row.Employee?.Role ?? "").Append(' ');
            combined.Append(row.Login ?? "").Append(' ');
            combined.Append(row.UserId.ToString());
            var norm = Normalize(combined.ToString());
            return tokens.All(t => norm.Contains(t));
        }).ToList();

        Users.Clear();
        foreach (var u in filtered) Users.Add(u);
    }

    private void ToggleEdit()
    {
        if (!IsEdit)
        {
            ClearErrors();
            SelectedEmployee = null;
            Login = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            IsEdit = true;
            return;
        }

        ClearForm();
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
