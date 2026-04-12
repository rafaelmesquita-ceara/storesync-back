using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

public partial class CommissionsViewModel : ObservableObject
{
    private readonly ICommissionService _commissionService;
    private readonly IEmployeeService _employeeService;
    private readonly IFinanceService _financeService;

    public ObservableCollection<CommissionViewModel> Commissions { get; } = new();
    public ObservableCollection<Employee> Employees { get; } = new();
    private List<CommissionViewModel>? _allCommissions;

    [ObservableProperty] private string _searchBarField = string.Empty;

    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _totalCount = 0;
    private int _pageSize = 50;

    public bool CanPreviousPage => CurrentPage > 1;
    public bool CanNextPage => CurrentPage < TotalPages;

    [RelayCommand(CanExecute = nameof(CanPreviousPage))]
    private async Task PreviousPage()
    {
        CurrentPage--;
        await LoadDataAsync();
    }

    [RelayCommand(CanExecute = nameof(CanNextPage))]
    private async Task NextPage()
    {
        CurrentPage++;
        await LoadDataAsync();
    }

    [ObservableProperty] private bool _isEdit;
    [ObservableProperty] private bool _isViewOnly;
    [ObservableProperty] private bool _isPreview;

    public bool ShowConfirmButton => IsPreview && !IsViewOnly;

    // Form fields
    [ObservableProperty] private string _reference = string.Empty;
    [ObservableProperty] private Employee? _selectedEmployee;
    [ObservableProperty] private DateTimeOffset? _startDate;
    [ObservableProperty] private DateTimeOffset? _endDate;
    [ObservableProperty] private string _observation = string.Empty;

    // Preview / view fields (read-only)
    [ObservableProperty] private string _totalSalesDisplay = string.Empty;
    [ObservableProperty] private string _commissionRateDisplay = string.Empty;
    [ObservableProperty] private string _commissionValueDisplay = string.Empty;

    // Calculated values stored for confirmation
    private decimal _calculatedTotalSales;
    private decimal _calculatedRate;
    private decimal _calculatedValue;

    // ID set when viewing existing record
    private Guid _editingCommissionId = Guid.Empty;

    public CommissionsViewModel(ICommissionService commissionService, IEmployeeService employeeService, IFinanceService financeService)
    {
        _commissionService = commissionService;
        _employeeService = employeeService;
        _financeService = financeService;
    }

    public async Task LoadDataAsync()
    {
        var offset = (CurrentPage - 1) * _pageSize;
        var paginatedResult = await _commissionService.GetAllCommissionsAsync(_pageSize, offset);

        TotalCount = paginatedResult.TotalCount;
        TotalPages = (int)Math.Ceiling((double)TotalCount / _pageSize);
        if (TotalPages == 0) TotalPages = 1;

        Commissions.Clear();
        foreach (var c in paginatedResult.Items)
            Commissions.Add(new CommissionViewModel(c));

        PreviousPageCommand.NotifyCanExecuteChanged();
        NextPageCommand.NotifyCanExecuteChanged();

        _allCommissions = Commissions.ToList();

        var employeesPage = await _employeeService.GetAllEmployeesAsync(1000, 0);
        Employees.Clear();
        foreach (var e in employeesPage.Items)
            Employees.Add(e);
    }

    [RelayCommand]
    public void OpenNew()
    {
        ClearForm();
        IsViewOnly = false;
        IsPreview = false;
        IsEdit = true;
    }

    [RelayCommand]
    public void OpenView(Guid commissionId)
    {
        var vm = Commissions.FirstOrDefault(c => c.CommissionId == commissionId);
        if (vm == null) return;

        _editingCommissionId = vm.CommissionId;
        Reference            = vm.Reference;
        SelectedEmployee     = Employees.FirstOrDefault(e => e.EmployeeId == vm.Model.EmployeeId);
        StartDate            = new DateTimeOffset(vm.StartDate, TimeSpan.Zero);
        EndDate              = new DateTimeOffset(vm.EndDate, TimeSpan.Zero);
        Observation          = vm.Observation ?? string.Empty;

        TotalSalesDisplay      = vm.TotalSales.ToString("N2", CultureInfo.CurrentCulture);
        CommissionRateDisplay  = vm.CommissionRate.ToString("N2", CultureInfo.CurrentCulture);
        CommissionValueDisplay = vm.CommissionValue.ToString("N2", CultureInfo.CurrentCulture);

        IsPreview  = true;
        IsViewOnly = true;
        IsEdit     = true;
    }

    [RelayCommand]
    public async Task Delete(Guid commissionId)
    {
        await _commissionService.DeleteCommissionAsync(commissionId);
        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task Calculate()
    {
        if (string.IsNullOrWhiteSpace(Reference))
        {
            StoreSyncFront.Services.SnackBarService.Send("Informe a referência da comissão.");
            return;
        }

        if (SelectedEmployee == null)
        {
            StoreSyncFront.Services.SnackBarService.Send("Selecione um funcionário.");
            return;
        }

        if (StartDate == null)
        {
            StoreSyncFront.Services.SnackBarService.Send("Informe a data inicial.");
            return;
        }

        if (EndDate == null)
        {
            StoreSyncFront.Services.SnackBarService.Send("Informe a data final.");
            return;
        }

        if (StartDate.Value.DateTime > EndDate.Value.DateTime)
        {
            StoreSyncFront.Services.SnackBarService.Send("Data inicial não pode ser maior que a data final.");
            return;
        }

        var (totalSales, rate, value) = await _commissionService.CalculateAsync(
            SelectedEmployee!.EmployeeId,
            StartDate.Value.DateTime,
            EndDate.Value.DateTime);

        if (totalSales == 0 && rate == 0 && value == 0)
            return; // erro já notificado no service

        _calculatedTotalSales = totalSales;
        _calculatedRate       = rate;
        _calculatedValue      = value;

        TotalSalesDisplay      = totalSales.ToString("N2", CultureInfo.CurrentCulture);
        CommissionRateDisplay  = rate.ToString("N2", CultureInfo.CurrentCulture);
        CommissionValueDisplay = value.ToString("N2", CultureInfo.CurrentCulture);

        IsPreview = true;
    }

    /// <summary>
    /// Chamado pelo code-behind após o fluxo de confirmação (conta a pagar + baixa).
    /// </summary>
    public async Task ConfirmCreateAsync()
    {
        var commission = new Commission
        {
            EmployeeId    = SelectedEmployee!.EmployeeId,
            Reference     = Reference,
            StartDate     = StartDate!.Value.DateTime,
            EndDate       = EndDate!.Value.DateTime,
            Observation   = string.IsNullOrWhiteSpace(Observation) ? null : Observation,
            TotalSales    = _calculatedTotalSales,
            CommissionRate = _calculatedRate,
            CommissionValue = _calculatedValue
        };

        await _commissionService.CreateCommissionAsync(commission);
        ClearForm();
        await LoadDataAsync();
    }

    /// <summary>
    /// Cria a conta a pagar no financeiro e retorna o FinanceId gerado, ou null em caso de erro.
    /// </summary>
    public async Task<Guid?> CreateFinanceForCommissionAsync()
    {
        var financeId = Guid.NewGuid();
        var finance = new Finance
        {
            FinanceId   = financeId,
            Reference   = $"COM{Reference}",
            Description = "Conta a pagar gerada automaticamente pela rotina de comissionamento",
            Amount      = _calculatedValue,
            DueDate     = DateTime.Today,
            Type        = FinanceType.Pagar,
            Status      = FinanceStatus.Aberto,
            TitleType   = FinanceTitleType.Original
        };

        var result = await _financeService.CreateFinanceAsync(finance);
        return result == 0 ? financeId : null;
    }

    /// <summary>
    /// Liquida a conta a pagar pelo FinanceId.
    /// </summary>
    public async Task SettleFinanceAsync(Guid financeId)
    {
        await _financeService.SettleAsync(financeId, _calculatedValue, null);
    }

    [RelayCommand]
    public void ClearForm()
    {
        IsEdit            = false;
        IsViewOnly        = false;
        IsPreview         = false;
        _editingCommissionId = Guid.Empty;
        Reference         = string.Empty;
        SelectedEmployee  = null;
        StartDate         = null;
        EndDate           = null;
        Observation       = string.Empty;
        TotalSalesDisplay      = string.Empty;
        CommissionRateDisplay  = string.Empty;
        CommissionValueDisplay = string.Empty;
        _calculatedTotalSales  = 0;
        _calculatedRate        = 0;
        _calculatedValue       = 0;
    }

    [RelayCommand]
    public async Task Refresh()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    public void Search()
    {
        if (_allCommissions == null)
            _allCommissions = Commissions.ToList();

        var query = (SearchBarField ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            Commissions.Clear();
            foreach (var c in _allCommissions) Commissions.Add(c);
            return;
        }

        var tokens = Regex.Split(query, @"\s+")
                          .Where(t => !string.IsNullOrWhiteSpace(t))
                          .Select(Normalize)
                          .ToArray();

        var filtered = _allCommissions.Where(c =>
        {
            var combined = new StringBuilder();
            combined.Append(c.Reference).Append(' ');
            combined.Append(c.EmployeeName).Append(' ');
            combined.Append(c.Period).Append(' ');
            combined.Append(c.CommissionValue.ToString(CultureInfo.InvariantCulture)).Append(' ');
            var norm = Normalize(combined.ToString());
            return tokens.All(t => norm.Contains(t));
        }).ToList();

        Commissions.Clear();
        foreach (var c in filtered) Commissions.Add(c);
    }

    partial void OnIsPreviewChanged(bool value) => OnPropertyChanged(nameof(ShowConfirmButton));
    partial void OnIsViewOnlyChanged(bool value) => OnPropertyChanged(nameof(ShowConfirmButton));

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
