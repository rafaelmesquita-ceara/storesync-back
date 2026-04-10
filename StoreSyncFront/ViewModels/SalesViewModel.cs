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
using StoreSyncFront.Services;

namespace StoreSyncFront.ViewModels;

public partial class SalesViewModel : ObservableValidator
{
    private readonly ISaleService _saleService;
    private readonly ISaleItemService _saleItemService;
    private readonly IProductService _productService;
    private readonly IEmployeeService _employeeService;
    private readonly IAuthService _authService;

    public string Title => "Vendas";

    public ObservableCollection<SaleViewModel> Sales { get; } = new();
    public ObservableCollection<SaleItemViewModel> SaleItems { get; } = new();
    public ObservableCollection<Employee> Employees { get; } = new();
    private List<SaleViewModel>? _allSales;

    [ObservableProperty] private string _searchBarField = string.Empty;
    [ObservableProperty] private bool _isEdit;
    [ObservableProperty] private bool _isViewOnly;
    [ObservableProperty] private bool _isActionsExpanded;

    [ObservableProperty] private Guid _saleId = Guid.Empty;
    [ObservableProperty] private Employee? _selectedEmployee;
    [ObservableProperty] private string _discount = "0";
    [ObservableProperty] private string _addition = "0";
    [ObservableProperty] private decimal _totalAmount;
    [ObservableProperty] private int _selectedStatus;
    [ObservableProperty] private SaleItemViewModel? _selectedSaleItem;

    public IRelayCommand ToggleEditCommand { get; }

    public SalesViewModel(
        ISaleService saleService,
        ISaleItemService saleItemService,
        IProductService productService,
        IEmployeeService employeeService,
        IAuthService authService)
    {
        _saleService = saleService;
        _saleItemService = saleItemService;
        _productService = productService;
        _employeeService = employeeService;
        _authService = authService;
        ToggleEditCommand = new AsyncRelayCommand(CreateNewSaleAsync);

        SaleItems.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CanFinalize));
            OnPropertyChanged(nameof(CanCancel));
        };
    }

    public async Task LoadDataAsync()
    {
        var items = await _saleService.GetAllSalesAsync();
        Sales.Clear();
        foreach (var item in items)
            Sales.Add(new SaleViewModel(item));
        _allSales = Sales.ToList();

        var employees = await _employeeService.GetAllEmployeesAsync();
        Employees.Clear();
        foreach (var e in employees)
            Employees.Add(e);
    }

    private async Task CreateNewSaleAsync()
    {
        var loggedUser = _authService.GetLoggedUser();
        var defaultEmployee = Employees.FirstOrDefault(e =>
            loggedUser?.EmployeeId != null && e.EmployeeId == loggedUser.EmployeeId);

        var sale = new Sale
        {
            EmployeeId = defaultEmployee?.EmployeeId ?? (loggedUser?.EmployeeId ?? Guid.Empty),
            Items = null
        };

        var result = await _saleService.CreateSaleAsync(sale);
        if (result != 0)
            return;

        SaleId = sale.SaleId;
        SelectedEmployee = defaultEmployee ?? Employees.FirstOrDefault();
        Discount = "0";
        Addition = "0";
        TotalAmount = 0;
        SelectedStatus = SaleStatus.Aberta;
        SaleItems.Clear();
        IsViewOnly = false;
        IsActionsExpanded = false;
        IsEdit = true;
    }

    [RelayCommand]
    public void OpenEdit(Guid saleId)
    {
        ClearErrors();
        var vm = Sales.FirstOrDefault(s => s.SaleId == saleId);
        if (vm == null) return;

        if (vm.Status != SaleStatus.Aberta)
        {
            OpenView(saleId);
            return;
        }

        LoadSaleIntoForm(vm, viewOnly: false);
    }

    [RelayCommand]
    public void OpenView(Guid saleId)
    {
        ClearErrors();
        var vm = Sales.FirstOrDefault(s => s.SaleId == saleId);
        if (vm == null) return;

        LoadSaleIntoForm(vm, viewOnly: true);
    }

    private async void LoadSaleIntoForm(SaleViewModel vm, bool viewOnly)
    {
        SaleId = vm.SaleId;
        SelectedEmployee = Employees.FirstOrDefault(e => e.EmployeeId == vm.EmployeeId);
        Discount = vm.Discount.ToString(CultureInfo.CurrentCulture);
        Addition = vm.Addition.ToString(CultureInfo.CurrentCulture);
        TotalAmount = vm.TotalAmount;
        SelectedStatus = vm.Status;

        var items = await _saleItemService.GetSaleItemsBySaleIdAsync(vm.SaleId);
        SaleItems.Clear();
        foreach (var item in items)
            SaleItems.Add(new SaleItemViewModel(item));

        IsViewOnly = viewOnly;
        IsActionsExpanded = false;
        IsEdit = true;
    }

    [RelayCommand]
    public void ClearForm()
    {
        ClearErrors();
        IsEdit = false;
        IsViewOnly = false;
        IsActionsExpanded = false;
        SaleId = Guid.Empty;
        SelectedEmployee = null;
        Discount = "0";
        Addition = "0";
        TotalAmount = 0;
        SelectedStatus = SaleStatus.Aberta;
        SaleItems.Clear();
        SelectedSaleItem = null;
    }

    [RelayCommand]
    public async Task Save()
    {
        if (IsViewOnly || SaleId == Guid.Empty) return;

        decimal.TryParse(Discount.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal discountValue);
        decimal.TryParse(Addition.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal additionValue);

        var sale = new Sale
        {
            SaleId = SaleId,
            EmployeeId = SelectedEmployee?.EmployeeId ?? Guid.Empty,
            Discount = discountValue,
            Addition = additionValue,
            TotalAmount = TotalAmount,
            Items = null
        };

        await _saleService.UpdateSaleAsync(sale);

        var refreshed = await _saleService.GetSaleByIdAsync(SaleId);
        if (refreshed != null)
            TotalAmount = refreshed.TotalAmount;

        await LoadDataAsync();
    }

    public async Task<Product?> AddItemAsync(Product product, int quantity, decimal itemDiscount, decimal itemAddition)
    {
        if (SaleId == Guid.Empty) return null;

        var saleItem = new SaleItem
        {
            SaleId = SaleId,
            ProductId = product.ProductId,
            Quantity = quantity,
            Discount = itemDiscount,
            Addition = itemAddition,
            TotalPrice = 0
        };

        var result = await _saleItemService.CreateSaleItemAsync(saleItem);
        if (result != 0) return null;

        var items = await _saleItemService.GetSaleItemsBySaleIdAsync(SaleId);
        SaleItems.Clear();
        foreach (var item in items)
            SaleItems.Add(new SaleItemViewModel(item));

        var refreshed = await _saleService.GetSaleByIdAsync(SaleId);
        if (refreshed != null)
            TotalAmount = refreshed.TotalAmount;

        return product;
    }

    [RelayCommand]
    public async Task RemoveItem()
    {
        if (SelectedSaleItem == null || SaleId == Guid.Empty) return;

        var result = await _saleItemService.DeleteSaleItemAsync(SelectedSaleItem.SaleItemId);
        if (result != 0) return;

        var items = await _saleItemService.GetSaleItemsBySaleIdAsync(SaleId);
        SaleItems.Clear();
        foreach (var item in items)
            SaleItems.Add(new SaleItemViewModel(item));

        var refreshed = await _saleService.GetSaleByIdAsync(SaleId);
        if (refreshed != null)
            TotalAmount = refreshed.TotalAmount;

        SelectedSaleItem = null;
    }

    public async Task FinalizeSaleAsync()
    {
        if (SaleId == Guid.Empty) return;

        var result = await _saleService.FinalizeSaleAsync(SaleId);
        if (result == 0)
        {
            ClearForm();
            await LoadDataAsync();
        }
    }

    public async Task CancelSaleAsync()
    {
        if (SaleId == Guid.Empty) return;

        var result = await _saleService.CancelSaleAsync(SaleId);
        if (result == 0)
        {
            ClearForm();
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    public async Task Refresh()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    public void Search()
    {
        if (_allSales == null)
            _allSales = Sales.ToList();

        var query = (SearchBarField ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            Sales.Clear();
            foreach (var s in _allSales) Sales.Add(s);
            return;
        }

        var tokens = Regex.Split(query, @"\s+")
                          .Where(t => !string.IsNullOrWhiteSpace(t))
                          .Select(Normalize)
                          .ToArray();

        var filtered = _allSales.Where(s =>
        {
            var combined = new StringBuilder();
            combined.Append(s.Referencia ?? string.Empty).Append(' ');
            combined.Append(s.EmployeeName ?? string.Empty).Append(' ');
            combined.Append(s.TotalAmount.ToString(CultureInfo.InvariantCulture)).Append(' ');
            combined.Append(s.SaleDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append(' ');
            combined.Append(s.StatusLabel);
            var norm = Normalize(combined.ToString());
            return tokens.All(t => norm.Contains(t));
        }).ToList();

        Sales.Clear();
        foreach (var s in filtered) Sales.Add(s);
    }

    [RelayCommand]
    public void ToggleActions()
    {
        IsActionsExpanded = !IsActionsExpanded;
    }

    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        return await _productService.GetAllProductsAsync();
    }

    public bool CanFinalize => SelectedStatus == SaleStatus.Aberta && SaleItems.Count > 0;
    public bool CanCancel => SelectedStatus != SaleStatus.Cancelada;

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is nameof(SelectedStatus) or nameof(SaleItems))
        {
            OnPropertyChanged(nameof(CanFinalize));
            OnPropertyChanged(nameof(CanCancel));
        }
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
