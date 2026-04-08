using System;
using System.Net.Http;
using Avalonia.Controls;
using StoreSyncFront.Services;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Views;

public partial class MainWindow : Window
{
    private INavigationService _navigationService;
    public MainWindow(MainWindowViewModel viewModel, INavigationService navigationService)
    {
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        this.CanResize = true;
        InitializeComponent();
        DataContext = viewModel;
        
        this._navigationService = navigationService;
        navigationService.Initialize(ContentHost);
        
#if DEBUG
        // Para desenvolvimento, pule o login e vá direto para a tela principal.
        navigationService.NavigateTo<LoginViewModel>();
#else
        // Em produção, o fluxo normal de login é mantido.
        navigationService.NavigateTo<LoginViewModel>();
#endif
    }
}