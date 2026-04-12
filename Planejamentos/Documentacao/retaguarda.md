# Retaguarda (Frontend Desktop)

**StoreSyncFront** — cliente desktop Avalonia UI, MVVM, comunicação exclusiva com StoreSyncBack via REST.

Stack: Avalonia UI 11 + CommunityToolkit.Mvvm + Material.Avalonia + ReactiveUI + Newtonsoft.Json

## Arquitetura MVVM

```
View (AXAML) → ViewModel (ObservableObject) → Service → StoreSyncBack (API)
```

- **Views** — layout e bindings, sem lógica
- **ViewModels** — estado, validação, comandos (`[ObservableProperty]`, `[RelayCommand]`)
- **Services** — HTTP com o backend; `ApiService` (client com Bearer), `AuthService` (sessão), `NavigationService`

## Sessão e autenticação

1. Login: POST `/api/Users/login` → token JWT armazenado via `ApiService.SetApiKey()`
2. Sessão persistida em `user_data.json` (local do executável, no `.gitignore`)
3. Login automático na próxima abertura via `LoadUserDataAsync()`
4. Logout: limpa memória, remove token, deleta `user_data.json`

## Navegação

`NavigationService` usa pilha de ViewModels. Associação View↔ViewModel por convenção de nome (`ProductsViewModel` → `ProductsView`).

| Método | Comportamento |
|---|---|
| `NavigateTo<TViewModel>()` | Empilha e exibe |
| `NavigateToRoot<TViewModel>()` | Limpa pilha e navega |
| `NavigateToBack()` | Volta para a anterior |

## Adicionar nova tela

1. Criar `Views/NovaView.axaml` + `NovaView.axaml.cs`
2. Criar `ViewModels/NovaViewModel.cs` herdando de `ObservableObject`
3. Registrar em `ServiceCollectionExtensions.AddCommonServices()`
4. Navegar via `_navigationService.NavigateTo<NovaViewModel>()`

## Adicionar novo service de API

1. Criar `Services/NovoService.cs` implementando interface do `SharedModels`
2. Injetar `IApiService` no construtor
3. Registrar como `Singleton` em `ServiceCollectionExtensions`
4. Usar `SnackBarService.Send()` para feedback de erro/sucesso

## Configuração

URL do backend em `StoreSyncFront/appsettings.json` → `ApiSettings.BaseUrl`.
