# Retaguarda (Frontend Desktop)

Stack: Avalonia UI 11 + CommunityToolkit.Mvvm + Material.Avalonia + ReactiveUI + Newtonsoft.Json

Arquitetura: `View (AXAML) → ViewModel (ObservableObject) → Service → API REST`

- Views: layout/bindings, sem lógica
- ViewModels: estado, validação, comandos (`[ObservableProperty]`, `[RelayCommand]`)
- Services: HTTP com bearer token; `ApiService`, `AuthService`, `NavigationService`

## Sessão

Login → POST `/api/Users/login` → token salvo via `ApiService.SetApiKey()` + persistido em `user_data.json`. Login automático via `LoadUserDataAsync()`. Logout: limpa memória + deleta arquivo.

## Navegação

`NavigationService` usa pilha de ViewModels. Convenção: `ProductsViewModel` → `ProductsView`.

- `NavigateTo<T>()` — empilha
- `NavigateToRoot<T>()` — limpa pilha
- `NavigateToBack()` — volta

## Adicionar nova tela

1. `Views/NovaView.axaml` + `.axaml.cs`
2. `ViewModels/NovaViewModel.cs` herdando `ObservableObject`
3. Registrar em `ServiceCollectionExtensions.AddCommonServices()`
4. Navegar via `_navigationService.NavigateTo<NovaViewModel>()`

## Adicionar novo service

1. `Services/NovoService.cs` implementando interface do `SharedModels`
2. Injetar `IApiService`; registrar como `Singleton`
3. Usar `SnackBarService.Send()` para feedback

URL do backend: `StoreSyncFront/appsettings.json` → `ApiSettings.BaseUrl`
