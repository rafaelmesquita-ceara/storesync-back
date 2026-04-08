﻿using System;
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
using SharedModels.Interfaces;

namespace StoreSyncFront.ViewModels;


public partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    
    [ObservableProperty] 
    private string _username = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<TabItemViewModel> _tabs = new();
    
    [ObservableProperty]
    private TabItemViewModel? _selectedTab;
    
    public MainViewModel(INavigationService navigationService, IAuthService authService, IProductService productService, ICategoryService categoryService)
    {
        this._navigationService = navigationService;
        this._authService = authService;
        this._productService = productService;
        this._categoryService = categoryService;
        

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
        var homeVm = new HomeViewModel(Username);
        var homeTab = new TabItemViewModel("Início", homeVm, false, CloseTab);
        Tabs.Add(homeTab);
        SelectedTab = homeTab;
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
    private void CloseTab(TabItemViewModel tab)
    {
        Tabs.Remove(tab);
    }
}