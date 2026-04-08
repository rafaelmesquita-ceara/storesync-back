using System;
using System.Net.Http;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using StoreSyncFront.Services;
using StoreSyncFront.Views;
using ReactiveUI;

namespace StoreSyncFront.ViewModels;


public class MainWindowViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    public static string SnackBarName { get; } = "SnackbarHost1";
    public MainWindowViewModel(INavigationService navigationService)
    {
        this._navigationService = navigationService;
        _navigationService.NavigateTo<LoginViewModel>();
    }
}
