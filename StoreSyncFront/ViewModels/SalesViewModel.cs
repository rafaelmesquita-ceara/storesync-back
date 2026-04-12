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
    private readonly IClientService _clientService;

    public string Title => "Vendas";

    public ObservableCollection<SaleViewModel> Sales { get; } = new();
    public ObservableCollection<SaleItemViewModel> SaleItems { get; } = new();
    public ObservableCollection<Employee> Employees { get; } = new();
    public ObservableCollection<Client> Clients { get; } = new();
    private List<SaleViewModel>? _allSales;

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

    [ObservableProperty] private Guid _saleId = Guid.Empty;
    [ObservableProperty] private Employee? _selectedEmployee;
    [ObservableProperty] private Client? _selectedClient;
    [ObservableProperty] private string _discount = "0";
    [ObservableProperty] private string _addition = "0";
    [ObservableProperty] private decimal _totalAmount;
    [ObservableProperty] private int _selectedStatus;
    [ObservableProperty] private SaleItemViewModel? _selectedSaleItem;

    public ISaleService SaleService => _saleService;

    public Task<byte[]?> DownloadReportAsync(DateTime startDate, DateTime endDate)
    {
        return _saleService.DownloadSalesReportAsync(startDate, endDate);
    }

    public IRelayCommand ToggleEditCommand { get; }

    public SalesViewModel(
        ISaleService saleService,
        ISaleItemService saleItemService,
        IProductService productService,
        IEmployeeService employeeService,
        IAuthService authService,
        IClientService clientService)
    {
        _saleService = saleService;
        _saleItemService = saleItemService;
        _productService = productService;
        _employeeService = employeeService;
        _authService = authService;
        _clientService = clientService;
        ToggleEditCommand = new AsyncRelayCommand(CreateNewSaleAsync);

        SaleItems.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CanFinalize));
            OnPropertyChanged(nameof(CanCancel));
        };
    }

    public async Task LoadDataAsync()
    {
        var offset = (CurrentPage - 1) * _pageSize;
        var paginatedResult = await _saleService.GetAllSalesAsync(_pageSize, offset);

        TotalCount = paginatedResult.TotalCount;
        TotalPages = (int)Math.Ceiling((double)TotalCount / _pageSize);
        if (TotalPages == 0) TotalPages = 1;

        Sales.Clear();
        foreach (var item in paginatedResult.Items)
            Sales.Add(new SaleViewModel(item));

        PreviousPageCommand.NotifyCanExecuteChanged();
        NextPageCommand.NotifyCanExecuteChanged();

        _allSales = Sales.ToList();

        var employeesPage = await _employeeService.GetAllEmployeesAsync(1000, 0);
        Employees.Clear();
        foreach (var e in employeesPage.Items)
            Employees.Add(e);

        var clientsPage = await _clientService.GetAllClientsAsync(1000, 0);
        Clients.Clear();
        Clients.Add(new Client { ClientId = Guid.Empty, Name = "(nenhum)" });
        foreach (var c in clientsPage.Items)
            Clients.Add(c);
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
        SelectedClient = Clients.FirstOrDefault(c => c.ClientId == Guid.Empty);
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
        SelectedClient = vm.ClientId.HasValue && vm.ClientId != Guid.Empty
            ? Clients.FirstOrDefault(c => c.ClientId == vm.ClientId)
            : Clients.FirstOrDefault(c => c.ClientId == Guid.Empty);
        Discount = vm.Discount.ToString(CultureInfo.CurrentCulture);
        Addition = vm.Addition.ToString(CultureInfo.CurrentCulture);
        TotalAmount = vm.TotalAmount;
        SelectedStatus = vm.Status;

        var result = await _saleItemService.GetSaleItemsBySaleIdAsync(vm.SaleId, 1000, 0);
        SaleItems.Clear();
        foreach (var item in result.Items)
            SaleItems.Add(new SaleItemViewModel(item));

        IsViewOnly = viewOnly;
        IsActionsExpanded = false;
        IsEdit = true;
    }

    [RelayCommand]
    public async Task ClearForm()
    {
        if (IsEdit && !IsViewOnly && SaleId != Guid.Empty)
        {
            await Save();
        }

        ClearErrors();
        IsEdit = false;
        IsViewOnly = false;
        IsActionsExpanded = false;
        SaleId = Guid.Empty;
        SelectedEmployee = null;
        SelectedClient = null;
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

        var clientId = SelectedClient?.ClientId;
        var sale = new Sale
        {
            SaleId = SaleId,
            EmployeeId = SelectedEmployee?.EmployeeId ?? Guid.Empty,
            ClientId = clientId == Guid.Empty ? null : clientId,
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

        await Save();

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

        var paginatedItems1 = await _saleItemService.GetSaleItemsBySaleIdAsync(SaleId, 1000, 0);
        SaleItems.Clear();
        foreach (var item in paginatedItems1.Items)
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

        await Save();

        var result = await _saleItemService.DeleteSaleItemAsync(SelectedSaleItem.SaleItemId);
        if (result != 0) return;

        var paginatedItems2 = await _saleItemService.GetSaleItemsBySaleIdAsync(SaleId, 1000, 0);
        SaleItems.Clear();
        foreach (var item in paginatedItems2.Items)
            SaleItems.Add(new SaleItemViewModel(item));

        var refreshed = await _saleService.GetSaleByIdAsync(SaleId);
        if (refreshed != null)
            TotalAmount = refreshed.TotalAmount;

        SelectedSaleItem = null;
    }

    public async Task FinalizeSaleAsync()
    {
        if (SaleId == Guid.Empty) return;

        await Save();

        var result = await _saleService.FinalizeSaleAsync(SaleId);
        if (result == 0)
        {
            IsViewOnly = true;
            await ClearForm();
            await LoadDataAsync();
        }
    }

    public async Task CancelSaleAsync()
    {
        if (SaleId == Guid.Empty) return;

        await Save();

        var result = await _saleService.CancelSaleAsync(SaleId);
        if (result == 0)
        {
            IsViewOnly = true;
            await ClearForm();
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
        var result = await _productService.GetAllProductsAsync(1000, 0);
        return result.Items;
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
