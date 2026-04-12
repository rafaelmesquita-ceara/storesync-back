using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using StoreSyncFront.Services;
using StoreSyncFront.Views;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncFront.ViewModels;


public partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IEmployeeService _employeeService;
    private readonly IFinanceService _financeService;
    private readonly ISaleService _saleService;
    private readonly ISaleItemService _saleItemService;
    private readonly ICommissionService _commissionService;
    private readonly IClientService _clientService;
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly ISalePaymentService _salePaymentService;
    private readonly StoreSyncFront.Services.CaixaService _caixaService;

    [ObservableProperty]
    private string _username = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<TabItemViewModel> _tabs = new();
    
    [ObservableProperty]
    private TabItemViewModel? _selectedTab;
    
    public MainViewModel(INavigationService navigationService, IAuthService authService, IProductService productService, ICategoryService categoryService, IEmployeeService employeeService, IFinanceService financeService, ISaleService saleService, ISaleItemService saleItemService, ICommissionService commissionService, IClientService clientService, IPaymentMethodService paymentMethodService, ISalePaymentService salePaymentService, StoreSyncFront.Services.CaixaService caixaService)
    {
        this._navigationService = navigationService;
        this._authService = authService;
        this._productService = productService;
        this._categoryService = categoryService;
        this._employeeService = employeeService;
        this._financeService = financeService;
        this._saleService = saleService;
        this._saleItemService = saleItemService;
        this._commissionService = commissionService;
        this._clientService = clientService;
        this._paymentMethodService = paymentMethodService;
        this._salePaymentService = salePaymentService;
        this._caixaService = caixaService;
        

        var loggedUser = _authService.GetLoggedUser();
        if (loggedUser != null)
        {
            _username = loggedUser.Employee?.Name ?? "Usuário";
        }
        else
        {
            // Define um valor padrão para o modo de design/debug
            _username = "Usuário de Teste";
        }

        // Criar e adicionar a aba inicial (Home)
        var homeVm = new HomeViewModel(Username, _saleService, _financeService, _productService, _categoryService, _employeeService);
        var homeTab = new TabItemViewModel("Início", homeVm, false, CloseTab);
        Tabs.Add(homeTab);
        SelectedTab = homeTab;
        _ = homeVm.LoadDataAsync();
    }
    
    [RelayCommand]
    private void Logout()
    {
        _authService.Logout(); // Limpa a sessão do usuário
        _navigationService.NavigateTo<LoginViewModel>();
    }

    [RelayCommand]
    private async Task OpenProductsTab()
    {
        // Evita abrir múltiplas abas iguais
        var existingTab = Tabs.FirstOrDefault(t => t.Content is ProductsViewModel);
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }
        
        var productsVm = new ProductsViewModel(_productService, _categoryService); 
        await productsVm.LoadDataAsync(); // Carrega os dados de forma assíncrona
        var productsTab = new TabItemViewModel("Produtos", productsVm, true, CloseTab);
        Tabs.Add(productsTab);
        SelectedTab = productsTab;
    }
    [RelayCommand]
    private async Task OpenCategoriesTab()
    {
        var existingTab = Tabs.FirstOrDefault(t => t.Content is CategoriesViewModel);
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        var categoriesVm = new CategoriesViewModel(_categoryService);
        await categoriesVm.LoadDataAsync();
        var categoriesTab = new TabItemViewModel("Categorias", categoriesVm, true, CloseTab);
        Tabs.Add(categoriesTab);
        SelectedTab = categoriesTab;
    }

    [RelayCommand]
    private async Task OpenEmployeesTab()
    {
        var existingTab = Tabs.FirstOrDefault(t => t.Content is EmployeesViewModel);
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        var employeesVm = new EmployeesViewModel(_employeeService);
        await employeesVm.LoadDataAsync();
        var employeesTab = new TabItemViewModel("Funcionários", employeesVm, true, CloseTab);
        Tabs.Add(employeesTab);
        SelectedTab = employeesTab;
    }

    [RelayCommand]
    private async Task OpenUsersTab()
    {
        var existingTab = Tabs.FirstOrDefault(t => t.Content is UsersViewModel);
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        var usersVm = new UsersViewModel(_authService, _employeeService);
        await usersVm.LoadDataAsync();
        var usersTab = new TabItemViewModel("Usuários", usersVm, true, CloseTab);
        Tabs.Add(usersTab);
        SelectedTab = usersTab;
    }

    [RelayCommand]
    private async Task OpenAccountsPayableTab()
    {
        var existingTab = Tabs.FirstOrDefault(t => t.Content is FinancesViewModel vm && vm.Title == "Contas a Pagar");
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        var financesVm = new FinancesViewModel(_financeService, FinanceType.Pagar);
        await financesVm.LoadDataAsync();
        var tab = new TabItemViewModel("Contas a Pagar", financesVm, true, CloseTab);
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    [RelayCommand]
    private async Task OpenAccountsReceivableTab()
    {
        var existingTab = Tabs.FirstOrDefault(t => t.Content is FinancesViewModel vm && vm.Title == "Contas a Receber");
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        var financesVm = new FinancesViewModel(_financeService, FinanceType.Receber);
        await financesVm.LoadDataAsync();
        var tab = new TabItemViewModel("Contas a Receber", financesVm, true, CloseTab);
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    [RelayCommand]
    private async Task OpenSalesTab()
    {
        var existingTab = Tabs.FirstOrDefault(t => t.Content is SalesViewModel);
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        var salesVm = new SalesViewModel(_saleService, _saleItemService, _productService, _employeeService, _authService, _clientService, _salePaymentService, _paymentMethodService, _caixaService);
        await salesVm.LoadDataAsync();
        var tab = new TabItemViewModel("Vendas", salesVm, true, CloseTab);
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    [RelayCommand]
    private async Task OpenCommissionsTab()
    {
        var existingTab = Tabs.FirstOrDefault(t => t.Content is CommissionsViewModel);
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        var vm = new CommissionsViewModel(_commissionService, _employeeService, _financeService);
        await vm.LoadDataAsync();
        var tab = new TabItemViewModel("Comissões", vm, true, CloseTab);
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    [RelayCommand]
    private async Task OpenClientsTab()
    {
        var existingTab = Tabs.FirstOrDefault(t => t.Content is ClientsViewModel);
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        var clientsVm = new ClientsViewModel(_clientService);
        await clientsVm.LoadDataAsync();
        var tab = new TabItemViewModel("Clientes", clientsVm, true, CloseTab);
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    [RelayCommand]
    private async Task OpenCaixasTab()
    {
        var existingTab = Tabs.FirstOrDefault(t => t.Content is CaixasViewModel);
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        var vm = new CaixasViewModel(_caixaService);
        await vm.LoadDataAsync();
        var tab = new TabItemViewModel("Caixas", vm, true, CloseTab);
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    [RelayCommand]
    private async Task OpenPaymentMethodsTab()
    {
        var existingTab = Tabs.FirstOrDefault(t => t.Content is PaymentMethodsViewModel);
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        var vm = new PaymentMethodsViewModel(_paymentMethodService);
        await vm.LoadDataAsync();
        var tab = new TabItemViewModel("Formas de Pagamento", vm, true, CloseTab);
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    private void CloseTab(TabItemViewModel tab)
    {
        Tabs.Remove(tab);
    }
}