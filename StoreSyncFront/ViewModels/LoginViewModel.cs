using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Styles.Controls;
using Material.Styles.Models;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Services;

namespace StoreSyncFront.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty] private string _login = string.Empty;

    [ObservableProperty] private string _senha = string.Empty;

    public LoginViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
    }
    
    public async Task InitializeAsync()
    {
        bool logged = await _authService.LoadUserDataAsync();
        if (logged)
        {
            SnackBarService.Send("Login automático realizado com sucesso.");
            _navigationService.NavigateTo<MainViewModel>();
        }
    }

    [RelayCommand]
    private async Task Entrar()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Senha))
        {
            SnackBarService.Send("Os campos devem estar preenchidos!");
            return;
        }

        string errorMessage = await _authService.Auth(new UserLoginDto() { Login = Login, Password = Senha });

        SnackBarService.Send(errorMessage == string.Empty ? "Login realizado com sucesso." : "Erro: " + errorMessage);
        if (errorMessage == string.Empty)
        {
            Console.WriteLine(_authService.GetLoggedUser().UserId);
            _navigationService.NavigateTo<MainViewModel>();
        }
        
    }
}