# StoreSync — Retaguarda (Desktop)

## O que é

O **StoreSyncFront** é o cliente desktop da aplicação, desenvolvido com **Avalonia UI**. Ele serve como a interface de retaguarda do sistema — onde os gestores realizam os cadastros, consultas e movimentações do dia a dia (produtos, categorias, funcionários, etc.).

Roda em **Windows** (WinExe) e se comunica exclusivamente com o **StoreSyncBack** via API REST.

---

## Stack técnica

| Tecnologia | Versão | Função |
|---|---|---|
| Avalonia UI | 11.2.1 | Framework de UI desktop multiplataforma |
| CommunityToolkit.Mvvm | 8.2.1 | MVVM + source generators (`[ObservableProperty]`, `[RelayCommand]`) |
| Material.Avalonia | 3.9.2 | Tema visual Material Design |
| ReactiveUI | 20.1.63 | Suporte a bindings reativos |
| Newtonsoft.Json | 13.0.3 | Desserialização das respostas da API |
| Microsoft.Extensions.DependencyInjection | 10.x | Injeção de dependência |

---

## Estrutura do projeto

```
StoreSyncFront/
├── Assets/                  ← Ícones e imagens da UI
├── Controls/                ← Controles reutilizáveis (ex: GenericFormControl)
├── Models/                  ← Modelos exclusivos do frontend (Response, ProductViewModel)
├── Services/                ← Serviços de integração com a API
│   ├── ApiService.cs        ← HTTP client com autenticação Bearer
│   ├── AuthService.cs       ← Autenticação, sessão e persistência local
│   ├── CategoryService.cs   ← CRUD de categorias via API
│   ├── ProductService.cs    ← CRUD de produtos via API
│   ├── NavigationService.cs ← Navegação entre ViewModels/Views
│   ├── SnackBarService.cs   ← Notificações toast para o usuário
│   └── ServiceCollectionExtensions.cs ← Registro de DI
├── Utils/                   ← Conversores (ex: InverseBooleanConverter)
├── ViewModels/              ← Lógica de apresentação (MVVM)
│   ├── LoginViewModel.cs
│   ├── MainViewModel.cs
│   ├── ProductsViewModel.cs
│   └── HomeViewModel.cs
├── Views/                   ← XAML das telas
│   ├── LoginView.axaml
│   ├── MainView.axaml
│   ├── MainWindow.axaml
│   └── ProductsView.axaml
├── appsettings.json         ← URL base da API
└── StoreSyncFront.csproj
```

---

## Arquitetura

Padrão **MVVM** com navegação por serviço:

```
View (AXAML)
    │  bindings compilados
    ▼
ViewModel (ObservableObject)
    │  chama
    ▼
Service (IApiService, IAuthService, IProductService...)
    │  HTTP REST
    ▼
StoreSyncBack (API)
```

- **Views** — apenas layout e bindings, sem lógica
- **ViewModels** — estado da tela, validação de formulário, comandos
- **Services** — comunicação HTTP com o backend, persistência de sessão

---

## Configuração

A URL do backend é definida em `appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5269"
  }
}
```

Para apontar para outro ambiente (homologação, produção), basta alterar o `BaseUrl`.

---

## Autenticação e sessão

O fluxo de autenticação é gerenciado pelo `AuthService`:

1. **Login manual** — o usuário preenche login e senha na `LoginView`
2. **`AuthService.Auth()`** — faz POST em `/api/Users/login`, recebe o objeto `User` com o token JWT
3. O token é armazenado via `ApiService.SetApiKey()` (header `Authorization: Bearer <token>`)
4. A sessão é persistida em `user_data.json` (local do executável)
5. **Login automático** — na próxima abertura, `LoadUserDataAsync()` restaura a sessão sem precisar logar novamente
6. **Logout** — limpa a memória, remove o token do `ApiService` e deleta o `user_data.json`

> `user_data.json` está no `.gitignore` — nunca deve ser versionado pois contém o token JWT do usuário.

---

## Navegação

O `NavigationService` gerencia a navegação entre telas usando uma pilha (`Stack`):

| Método | Comportamento |
|---|---|
| `NavigateTo<TViewModel>()` | Empilha e exibe a view associada ao ViewModel |
| `NavigateToRoot<TViewModel>()` | Limpa a pilha e navega para o ViewModel |
| `NavigateToBack()` | Desempilha e volta para a tela anterior |

A associação View ↔ ViewModel é feita por convenção de nome: `ProductsViewModel` → `ProductsView`.

---

## Telas implementadas

### Login (`LoginView`)
- Campos de login e senha
- Botão "Entrar" com validação de campos obrigatórios
- Login automático via sessão persistida
- Feedback visual via SnackBar

### Home (`HomeView`)
- Tela de boas-vindas exibindo o nome do funcionário logado
- Ponto de entrada após autenticação

### Principal (`MainView` + `MainWindow`)
- Layout com abas (`TabControl`)
- Barra lateral com ações de navegação
- Botão de Logout
- Abas fecháveis (exceto a aba "Início")

### Produtos (`ProductsView`)
- Listagem de todos os produtos com colunas: Referência, Nome, Categoria, Estoque, Preço, Data de cadastro
- Busca em tempo real (multi-token, insensível a acentos)
- Formulário lateral para criação e edição:
  - Campos: Referência, Nome, Estoque, Preço, Categoria (dropdown)
  - Validação com `[Required]` e `[RegularExpression]` via `ObservableValidator`
  - Modo criação / modo edição alternado pelo mesmo formulário
- Exclusão de produto com atualização automática da lista

---

## Funcionalidades previstas (próximas telas)

| Tela | Módulo | Status |
|---|---|---|
| Produtos | Estoque | Implementado |
| Categorias | Estoque | A implementar |
| Funcionários | RH | A implementar |
| PDV / Vendas | Vendas | A implementar |
| Comissões | Financeiro | A implementar |
| Financeiro | Financeiro | A implementar |

---

## Como rodar

### Pré-requisitos
- .NET 9 SDK instalado
- StoreSyncBack rodando em `http://localhost:5269`

### Executar

```bash
dotnet run --project StoreSyncFront/StoreSyncFront.csproj
```

### Build de produção

```bash
dotnet publish StoreSyncFront/StoreSyncFront.csproj -c Release -r win-x64 --self-contained
```

---

## Padrões de desenvolvimento

### Adicionar uma nova tela

1. Criar `Views/NovaView.axaml` + `Views/NovaView.axaml.cs`
2. Criar `ViewModels/NovaViewModel.cs` herdando de `ObservableObject`
3. Registrar o ViewModel em `ServiceCollectionExtensions.AddCommonServices()`
4. Navegar via `_navigationService.NavigateTo<NovaViewModel>()`

### Adicionar um novo service de API

1. Criar `Services/NovoService.cs` implementando a interface do `SharedModels`
2. Injetar `IApiService` no construtor
3. Registrar como `Singleton` em `ServiceCollectionExtensions`
4. Usar `SnackBarService.Send()` para feedback ao usuário em caso de erro/sucesso

### Notificações ao usuário

```csharp
SnackBarService.Send("Produto salvo com sucesso.");
SnackBarService.Send("Erro ao salvar: " + response.Body);
```

As notificações aparecem como um *snackbar* no rodapé da janela por 8 segundos.
