using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.ViewModels.Dashboard;

namespace StoreSyncFront.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly ISaleService _saleService;
    private readonly IFinanceService _financeService;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IEmployeeService _employeeService;
    private readonly ISaleItemService _saleItemService;
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly ISalePaymentService _salePaymentService;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private int _currentPageIndex;

    public ObservableCollection<DashboardPageViewModelBase> DashboardPages { get; } = new();

    public HomeViewModel(
        string username,
        ISaleService saleService,
        IFinanceService financeService,
        IProductService productService,
        ICategoryService categoryService,
        IEmployeeService employeeService,
        ISaleItemService saleItemService,
        IPaymentMethodService paymentMethodService,
        ISalePaymentService salePaymentService)
    {
        _username = username;
        _saleService = saleService;
        _financeService = financeService;
        _productService = productService;
        _categoryService = categoryService;
        _employeeService = employeeService;
        _saleItemService = saleItemService;
        _paymentMethodService = paymentMethodService;
        _salePaymentService = salePaymentService;

        DashboardPages.Add(new VisaoGeralDashboardViewModel());
        DashboardPages.Add(new FinanceiroDashboardViewModel());
        DashboardPages.Add(new VendasDashboardViewModel());
        DashboardPages.Add(new EstoqueDashboardViewModel());
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var salesTask = _saleService.GetAllSalesAsync(int.MaxValue, 0);
            var financeTask = _financeService.GetAllFinanceAsync(int.MaxValue, 0);
            var productsTask = _productService.GetAllProductsAsync(int.MaxValue, 0);
            var categoriesTask = _categoryService.GetAllCategoriesAsync(int.MaxValue, 0);
            var employeesTask = _employeeService.GetAllEmployeesAsync(int.MaxValue, 0);
            var saleItemsTask = _saleItemService.GetAllSaleItemsAsync(int.MaxValue, 0);
            var paymentMethodsTask = _paymentMethodService.GetAllAsync();
            var salePaymentsTask = _salePaymentService.GetAllSalePaymentsAsync(int.MaxValue, 0);

            await Task.WhenAll(salesTask, financeTask, productsTask, categoriesTask,
                employeesTask, saleItemsTask, paymentMethodsTask, salePaymentsTask);

            var bundle = new DashboardDataBundle(
                Sales: (await salesTask).Items.ToList(),
                Finances: (await financeTask).Items.ToList(),
                Products: (await productsTask).Items.ToList(),
                Categories: (await categoriesTask).Items.ToList(),
                Employees: (await employeesTask).Items.ToList(),
                SaleItems: (await saleItemsTask).Items.ToList(),
                SalePayments: (await salePaymentsTask).Items.ToList(),
                PaymentMethods: (await paymentMethodsTask).ToList()
            );

            foreach (var page in DashboardPages)
            {
                page.BuildFromData(bundle);
            }
        }
        catch
        {
            // mantém dashboards vazios sem derrubar a aplicação
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ReloadAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPageIndex < DashboardPages.Count - 1)
            CurrentPageIndex++;
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPageIndex > 0)
            CurrentPageIndex--;
    }

    [RelayCommand]
    private void GoToPage(object? param)
    {
        int index = param switch
        {
            int i => i,
            string s when int.TryParse(s, out var parsed) => parsed,
            _ => -1
        };

        if (index >= 0 && index < DashboardPages.Count)
            CurrentPageIndex = index;
    }
}
