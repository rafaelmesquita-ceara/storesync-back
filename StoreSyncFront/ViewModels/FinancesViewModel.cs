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

public partial class FinancesViewModel : ObservableValidator
{
    private readonly IFinanceService _financeService;
    private readonly int _financeType;

    public string Title => _financeType == FinanceType.Receber ? "Contas a Receber" : "Contas a Pagar";

    public ObservableCollection<FinanceViewModel> Finances { get; } = new();
    private List<FinanceViewModel>? _allFinances;

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
    [ObservableProperty] private bool _isActionsExpanded;

    // Selected record
    [ObservableProperty] private Guid _financeId = Guid.Empty;
    [ObservableProperty] private int _selectedStatus;

    // Form fields (editable)
    [ObservableProperty]
    [Required(ErrorMessage = "O campo Referência é obrigatório.")]
    private string _reference = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "O campo Descrição é obrigatória.")]
    private string _description = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "O campo Valor é obrigatório.")]
    [RegularExpression(@"^\d+([,.]\d{1,2})?$", ErrorMessage = "Insira um valor numérico válido.")]
    private string _amount = string.Empty;

    [ObservableProperty] private DateTimeOffset? _dueDate;

    // Settlement display fields (view-only)
    [ObservableProperty] private string _settledAmountDisplay = string.Empty;
    [ObservableProperty] private string _settledAtDisplay = string.Empty;
    [ObservableProperty] private string _settledNoteDisplay = string.Empty;
    [ObservableProperty] private bool _hasSettlementInfo;

    public IRelayCommand ToggleEditCommand { get; }

    public FinancesViewModel(IFinanceService financeService, int financeType)
    {
        _financeService = financeService;
        _financeType = financeType;
        ToggleEditCommand = new RelayCommand(() => IsEdit = !IsEdit);
    }

    public async Task LoadDataAsync()
    {
        var offset = (CurrentPage - 1) * _pageSize;
        var paginatedResult = await _financeService.GetAllByTypeAsync(_financeType, _pageSize, offset);

        TotalCount = paginatedResult.TotalCount;
        TotalPages = (int)Math.Ceiling((double)TotalCount / _pageSize);
        if (TotalPages == 0) TotalPages = 1;

        Finances.Clear();
        foreach (var item in paginatedResult.Items)
            Finances.Add(new FinanceViewModel(item));

        PreviousPageCommand.NotifyCanExecuteChanged();
        NextPageCommand.NotifyCanExecuteChanged();

        _allFinances = Finances.ToList();
    }

    [RelayCommand]
    public void OpenEdit(Guid financeId)
    {
        ClearErrors();
        var vm = Finances.FirstOrDefault(f => f.FinanceId == financeId);
        if (vm == null) return;

        if (vm.Status != FinanceStatus.Aberto)
        {
            StoreSyncFront.Services.SnackBarService.SendWarning("Apenas contas em aberto podem ser editadas.");
            return;
        }

        FinanceId         = vm.FinanceId;
        Reference         = vm.Reference ?? string.Empty;
        Description       = vm.Description ?? string.Empty;
        Amount            = vm.Amount.ToString(CultureInfo.CurrentCulture);
        DueDate           = vm.DueDate == default ? null : new DateTimeOffset(vm.DueDate);
        SelectedStatus    = vm.Status;
        IsViewOnly        = false;
        IsActionsExpanded = false;
        IsEdit            = true;
    }

    [RelayCommand]
    public void OpenView(Guid financeId)
    {
        ClearErrors();
        var vm = Finances.FirstOrDefault(f => f.FinanceId == financeId);
        if (vm == null) return;

        FinanceId      = vm.FinanceId;
        Reference      = vm.Reference ?? string.Empty;
        Description    = vm.Description ?? string.Empty;
        Amount         = vm.Amount.ToString("N2", CultureInfo.CurrentCulture);
        DueDate        = vm.DueDate == default ? null : new DateTimeOffset(vm.DueDate);
        SelectedStatus = vm.Status;

        HasSettlementInfo     = vm.SettledAmount.HasValue;
        SettledAmountDisplay  = vm.SettledAmount.HasValue
            ? vm.SettledAmount.Value.ToString("N2", CultureInfo.CurrentCulture)
            : string.Empty;
        SettledAtDisplay      = vm.SettledAt.HasValue
            ? vm.SettledAt.Value.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture)
            : string.Empty;
        SettledNoteDisplay    = vm.SettledNote ?? string.Empty;

        IsViewOnly        = true;
        IsActionsExpanded = false;
        IsEdit            = true;
    }

    [RelayCommand]
    public void ClearForm()
    {
        ClearErrors();
        IsEdit            = false;
        IsViewOnly        = false;
        IsActionsExpanded = false;
        FinanceId             = Guid.Empty;
        Reference             = string.Empty;
        Description           = string.Empty;
        Amount                = string.Empty;
        DueDate               = null;
        SelectedStatus        = FinanceStatus.Aberto;
        HasSettlementInfo     = false;
        SettledAmountDisplay  = string.Empty;
        SettledAtDisplay      = string.Empty;
        SettledNoteDisplay    = string.Empty;
    }

    [RelayCommand]
    public async Task Save()
    {
        if (IsViewOnly) return;

        ClearErrors();
        ValidateAllProperties();
        if (HasErrors) return;

        decimal.TryParse(
            Amount.Replace(',', '.'),
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out decimal amountValue);

        var finance = new Finance
        {
            Reference   = Reference,
            Description = Description,
            Amount      = amountValue,
            DueDate     = DueDate?.DateTime ?? BrazilDateTime.Now,
            Type        = _financeType,
            Status      = FinanceStatus.Aberto,
            TitleType   = FinanceTitleType.Original
        };

        if (FinanceId != Guid.Empty)
        {
            finance.FinanceId = FinanceId;
            await _financeService.UpdateFinanceAsync(finance);
        }
        else
        {
            await _financeService.CreateFinanceAsync(finance);
        }

        ClearForm();
        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task Delete(Guid financeId)
    {
        await _financeService.DeleteFinanceAsync(financeId);
        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task Refresh()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    public void Search()
    {
        if (_allFinances == null)
            _allFinances = Finances.ToList();

        var query = (SearchBarField ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            Finances.Clear();
            foreach (var f in _allFinances) Finances.Add(f);
            return;
        }

        var tokens = Regex.Split(query, @"\s+")
                          .Where(t => !string.IsNullOrWhiteSpace(t))
                          .Select(Normalize)
                          .ToArray();

        var filtered = _allFinances.Where(f =>
        {
            var combined = new StringBuilder();
            combined.Append(f.Reference ?? string.Empty).Append(' ');
            combined.Append(f.Description ?? string.Empty).Append(' ');
            combined.Append(f.Amount.ToString(CultureInfo.InvariantCulture)).Append(' ');
            combined.Append(f.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append(' ');
            combined.Append(f.StatusLabel).Append(' ');
            combined.Append(f.TitleTypeLabel).Append(' ');
            var norm = Normalize(combined.ToString());
            return tokens.All(t => norm.Contains(t));
        }).ToList();

        Finances.Clear();
        foreach (var f in filtered) Finances.Add(f);
    }

    [RelayCommand]
    public void ToggleActions()
    {
        IsActionsExpanded = !IsActionsExpanded;
    }

    // CanExecute helpers
    public bool CanSettle       => SelectedStatus == FinanceStatus.Aberto || SelectedStatus == FinanceStatus.LiquidadoParcialmente;
    public bool CanCancelSettle => SelectedStatus == FinanceStatus.Liquidado || SelectedStatus == FinanceStatus.LiquidadoParcialmente;

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(SelectedStatus))
        {
            OnPropertyChanged(nameof(CanSettle));
            OnPropertyChanged(nameof(CanCancelSettle));
        }
    }

    public async Task ConfirmSettleAsync(decimal settledAmount, string? note)
    {
        await _financeService.SettleAsync(FinanceId, settledAmount, note);
        ClearForm();
        await LoadDataAsync();
    }

    public async Task ConfirmCancelSettlementAsync()
    {
        await _financeService.CancelSettlementAsync(FinanceId);
        ClearForm();
        await LoadDataAsync();
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
