using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using StoreSyncFront.Services;
using StoreSyncFront.Views;
using ReactiveUI;
using Avalonia.Threading;
using SharedModels.Interfaces;

namespace StoreSyncFront.ViewModels;


public class MainWindowViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    
    public ObservableCollection<ToastModel> Toasts => SnackBarService.Toasts;
    
    public MainWindowViewModel(INavigationService navigationService, IApiService apiService, IAuthService authService)
    {
        this._navigationService = navigationService;
        
        apiService.OnUnauthorized += () => 
        {
            Dispatcher.UIThread.InvokeAsync(() => 
            {
                authService.Logout();
                _navigationService.NavigateTo<LoginViewModel>();
            });
        };
        
        _navigationService.NavigateTo<LoginViewModel>();
    }
}
