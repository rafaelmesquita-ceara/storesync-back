# Planejamento: Testes Unitários e TDD

## Visão Geral

Implementar uma suite completa de testes unitários no StoreSyncBack seguindo o padrão **Test-Driven Development (TDD)** para garantir qualidade, manutenibilidade e confiança no código.

---

## Contexto Atual

O projeto possui 8 services principais sem cobertura de testes:
- `UserService` - Autenticação, JWT, CRUD de usuários
- `CategoryService` - CRUD de categorias
- `EmployeeService` - Gestão de funcionários
- `ProductService` - Gestão de produtos
- `SaleService` - Vendas
- `SaleItemService` - Itens de venda
- `CommissionService` - Comissões
- `FinanceService` - Financeiro
- `MigrationService` - Sistema de migrations

---

## Estrutura de Testes

### 1. Projeto de Testes

**Local:** `StoreSyncBack.Tests/StoreSyncBack.Tests.csproj`

**Estrutura de pastas:**
```
StoreSyncBack.Tests/
├── StoreSyncBack.Tests.csproj
├── Unit/
│   ├── Services/
│   │   ├── UserServiceTests.cs
│   │   ├── CategoryServiceTests.cs
│   │   ├── EmployeeServiceTests.cs
│   │   ├── ProductServiceTests.cs
│   │   ├── SaleServiceTests.cs
│   │   ├── SaleItemServiceTests.cs
│   │   ├── CommissionServiceTests.cs
│   │   ├── FinanceServiceTests.cs
│   │   └── MigrationServiceTests.cs
│   ├── Repositories/
│   │   └── (Testes de integração - opcional)
│   └── Validators/
│       └── (Testes para FluentValidation)
├── Fixtures/
│   └── TestData.cs
└── Helpers/
    └── MockHelpers.cs
```

### 2. Frameworks de Teste

| Framework | Propósito | Pacote NuGet |
|-----------|-----------|--------------|
| xUnit | Framework principal de testes | `xunit` |
| xUnit Runner | Runner para Visual Studio | `xunit.runner.visualstudio` |
| Moq | Mocking de dependências | `Moq` |
| FluentAssertions | Asserções legíveis | `FluentAssertions` |
| Bogus | Geração de dados fake | `Bogus` |

### 3. Estrutura Padrão de Teste

```csharp
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IOptions<JwtSettings>> _jwtOptionsMock;
    private readonly Mock<IValidator<UserLoginDto>> _loginValidatorMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        // Arrange (configuração comum)
        _userRepoMock = new Mock<IUserRepository>();
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _jwtOptionsMock = new Mock<IOptions<JwtSettings>>();
        _loginValidatorMock = new Mock<IValidator<UserLoginDto>>();
        
        _jwtOptionsMock.Setup(x => x.Value).Returns(new JwtSettings 
        { 
            Key = "test-key-minimum-32-characters-long", 
            Issuer = "Test", 
            Audience = "Test",
            ExpiresMinutes = 60 
        });

        _userService = new UserService(
            _userRepoMock.Object,
            _employeeRepoMock.Object,
            _jwtOptionsMock.Object,
            _loginValidatorMock.Object,
            Mock.Of<IValidator<UserChangePasswordDto>>()
        );
    }

    [Fact]
    public async Task GetUserByIdAsync_UsuarioExistente_RetornaUsuario()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new User { UserId = userId, Login = "test" };
        _userRepoMock.Setup(r => r.GetUserByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.Login.Should().Be("test");
    }
}
```

---

## Ciclo TDD

### Regra de Ouro

> **Vermelho → Verde → Refatorar**

1. **Vermelho**: Escrever o teste ANTES da implementação (deve falhar)
2. **Verde**: Implementar o mínimo necessário para o teste passar
3. **Refatorar**: Melhorar o código mantendo os testes verdes

### Processo para Novas Funcionalidades

```
┌─────────────────────────────────────────────────────────────┐
│  1. Análise                                                 │
│     - Entender o requisito                                  │
│     - Identificar comportamentos esperados                  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  2. Escrever Teste (Vermelho)                              │
│     - Criar classe de teste                                 │
│     - Escrever teste que define comportamento               │
│     - Executar e confirmar falha                            │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  3. Implementar Mínimo (Verde)                               │
│     - Criar classe/service mínimo                          │
│     - Fazer teste passar (não precisa ser perfeito)         │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  4. Refatorar                                               │
│     - Melhorar código                                       │
│     - Extrair métodos, renomear variáveis                   │
│     - Garantir que testes continuam passando                 │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  5. Próximo Teste                                           │
│     - Repetir para próximo cenário                          │
│     - Cobrir casos de erro, edge cases                      │
└─────────────────────────────────────────────────────────────┘
```

---

## Cobertura de Testes por Service

### Priority 1: UserService (Autenticação)

| Cenário | Tipo |
|---------|------|
| Login com credenciais válidas | Sucesso |
| Login com senha incorreta | Falha |
| Login com usuário inexistente | Falha |
| Login com DTO inválido | Exceção |
| CreateUser com dados válidos | Sucesso |
| CreateUser com login vazio | Exceção |
| CreateUser com senha vazia | Exceção |
| UpdateUser com sucesso | Sucesso |
| UpdateUser com ID inválido | Exceção |
| DeleteUser com sucesso | Sucesso |
| DeleteUser com ID vazio | Exceção |
| ChangePassword com dados válidos | Sucesso |
| ChangePassword com senha antiga incorreta | Exceção |
| GenerateJwtToken retorna token válido | Sucesso |

### Priority 2: CategoryService (CRUD Simples)

| Cenário | Tipo |
|---------|------|
| GetAll retorna todas categorias | Sucesso |
| GetById categoria existente | Sucesso |
| GetById categoria inexistente | Retorna null |
| CreateCategory com nome válido | Sucesso |
| CreateCategory com nome vazio | Exceção |
| CreateCategory com null | Exceção |
| UpdateCategory com dados válidos | Sucesso |
| UpdateCategory com ID vazio | Exceção |
| DeleteCategory com ID válido | Sucesso |
| DeleteCategory com ID vazio | Exceção |

### Priority 3: ProductService

| Cenário | Tipo |
|---------|------|
| CreateProduct com dados válidos | Sucesso |
| CreateProduct com preço negativo | Exceção |
| CreateProduct com estoque negativo | Exceção |
| CreateProduct com categoria inexistente | Exceção |
| UpdateProduct ajusta estoque | Sucesso |
| GetProductsByCategory retorna filtrado | Sucesso |

### Priority 4: SaleService (Regras de Negócio)

| Cenário | Tipo |
|---------|------|
| CreateSale calcula total corretamente | Sucesso |
| CreateSale com itens vazios | Exceção |
| CreateSale atualiza estoque | Sucesso |
| CreateSale sem estoque suficiente | Exceção |
| GetSalesByEmployee retorna vendas | Sucesso |
| GetSalesByDate retorna filtrado | Sucesso |

### Priority 5: CommissionService

| Cenário | Tipo |
|---------|------|
| CalculateCommission aplica taxa correta | Sucesso |
| CalculateCommission mês sem vendas | Zero |
| GetCommissionsByEmployee agrupa corretamente | Sucesso |

### Priority 6: MigrationService

| Cenário | Tipo |
|---------|------|
| ApplyMigrations tabela não existe | Cria tabela |
| ApplyMigrations sem migrations pendentes | Não faz nada |
| ApplyMigrations aplica migrations em ordem | Sucesso |
| ApplyMigrations falha em migration | Rollback |

---

## Configuração do Projeto de Testes

### StoreSyncBack.Tests.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <!-- Framework de testes -->
    <PackageReference Include="xunit" Version="2.9.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.*">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    
    <!-- Mocking -->
    <PackageReference Include="Moq" Version="4.20.*" />
    
    <!-- Asserções -->
    <PackageReference Include="FluentAssertions" Version="6.12.*" />
    
    <!-- Dados de teste -->
    <PackageReference Include="Bogus" Version="35.6.*" />
    
    <!-- Cobertura (opcional) -->
    <PackageReference Include="coverlet.collector" Version="6.0.*">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\StoreSyncBack\StoreSyncBack.csproj" />
  </ItemGroup>

</Project>
```

### Adicionar ao Solution

```bash
dotnet sln StoreSyncBack/StoreSyncBack.sln add StoreSyncBack.Tests/StoreSyncBack.Tests.csproj
```

---

## Padrões e Convenções

### Nomenclatura de Testes

**Padrão:** `[Metodo]_ [Estado]_ [ResultadoEsperado]`

Exemplos:
- `CreateCategoryAsync_NomeValido_RetornaIdCriado`
- `CreateCategoryAsync_NomeVazio_LancaArgumentException`
- `Login_CredenciaisValidas_RetornaUsuarioComToken`
- `Login_SenhaIncorreta_RetornaNull`

### Organização dos Testes

```csharp
public class CategoryServiceTests
{
    #region CreateCategoryAsync
    
    [Fact]
    public async Task CreateCategoryAsync_NomeValido_RetornaIdCriado() { }
    
    [Fact]
    public async Task CreateCategoryAsync_NomeVazio_LancaArgumentException() { }
    
    [Fact]
    public async Task CreateCategoryAsync_CategoriaNull_LancaArgumentNullException() { }
    
    #endregion

    #region GetCategoryByIdAsync
    
    [Fact]
    public async Task GetCategoryByIdAsync_IdExistente_RetornaCategoria() { }
    
    [Fact]
    public async Task GetCategoryByIdAsync_IdInexistente_RetornaNull() { }
    
    #endregion
}
```

### Dados de Teste (Bogus)

```csharp
public static class TestData
{
    private static readonly Faker<Category> CategoryFaker = new Faker<Category>()
        .RuleFor(c => c.CategoryId, f => Guid.NewGuid())
        .RuleFor(c => c.Name, f => f.Commerce.Categories(1).First())
        .RuleFor(c => c.CreatedAt, f => f.Date.Past());

    public static Category CreateCategory() => CategoryFaker.Generate();
    
    public static List<Category> CreateCategories(int count) => CategoryFaker.Generate(count);
}
```

---

## Atualização do CLAUDE.md

Adicionar seção sobre TDD:

```markdown
## Test-Driven Development (TDD)

Este projeto segue o padrão TDD. Toda implementação deve seguir o ciclo:

1. **Vermelho**: Escrever teste que falha antes da implementação
2. **Verde**: Implementar o mínimo para o teste passar
3. **Refatorar**: Melhorar código mantendo testes verdes

### Regras

- Nenhum código de produção sem teste correspondente
- Testes devem ser executados antes de cada commit
- Cobertura mínima: 80% dos services
- Usar xUnit + Moq + FluentAssertions

### Executando Testes

```bash
# Executar todos os testes
dotnet test StoreSyncBack.Tests/StoreSyncBack.Tests.csproj

# Executar com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Executar em modo watch (desenvolvimento)
dotnet watch test --project StoreSyncBack.Tests/StoreSyncBack.Tests.csproj
```

### Estrutura de Testes

- Local: `StoreSyncBack.Tests/Unit/Services/`
- Padrão: `[Metodo]_[Estado]_[Resultado]`
- Um arquivo de teste por service
```

---

## Plano de Implementação

### Fase 1: Setup (Dia 1)
- [ ] Criar projeto StoreSyncBack.Tests
- [ ] Configurar pacotes NuGet
- [ ] Criar estrutura de pastas
- [ ] Criar classe base de teste com mocks comuns
- [ ] Adicionar ao solution

### Fase 2: UserService (Dias 2-3)
- [ ] Testes para Login
- [ ] Testes para CreateUser
- [ ] Testes para UpdateUser
- [ ] Testes para DeleteUser
- [ ] Testes para ChangePassword

### Fase 3: CategoryService (Dia 4)
- [ ] Testes para CRUD completo

### Fase 4: Demais Services (Dias 5-7)
- [ ] ProductService
- [ ] EmployeeService
- [ ] SaleService
- [ ] SaleItemService
- [ ] CommissionService
- [ ] FinanceService
- [ ] MigrationService

### Fase 5: Integração (Dia 8)
- [ ] Configurar CI para executar testes
- [ ] Relatório de cobertura
- [ ] Documentar no CLAUDE.md

---

## Exemplo Completo de Implementação TDD

### Passo 1: Escrever Teste (Vermelho)

```csharp
[Fact]
public async Task CreateCategoryAsync_NomeValido_RetornaIdCriado()
{
    // Arrange
    var category = new Category { Name = "Eletrônicos" };
    _repoMock.Setup(r => r.CreateCategoryAsync(It.IsAny<Category>()))
        .ReturnsAsync(1);

    // Act
    var result = await _service.CreateCategoryAsync(category);

    // Assert
    result.Should().Be(1);
    _repoMock.Verify(r => r.CreateCategoryAsync(category), Times.Once);
}
```

### Passo 2: Implementar Mínimo (Verde)

```csharp
public async Task<int> CreateCategoryAsync(Category category)
{
    return await _repo.CreateCategoryAsync(category);
}
```

### Passo 3: Refatorar

```csharp
public async Task<int> CreateCategoryAsync(Category category)
{
    if (category == null)
        throw new ArgumentNullException(nameof(category));
    
    if (string.IsNullOrWhiteSpace(category.Name))
        throw new ArgumentException("Name é obrigatório", nameof(category.Name));

    if (category.CreatedAt == default)
        category.CreatedAt = DateTime.UtcNow;

    return await _repo.CreateCategoryAsync(category);
}
```

### Passo 4: Adicionar Mais Testes

```csharp
[Fact]
public async Task CreateCategoryAsync_NomeVazio_LancaArgumentException()
{
    // Arrange
    var category = new Category { Name = "" };

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(() => 
        _service.CreateCategoryAsync(category));
}
```

---

## Arquivos a Criar/Modificar

### Novos Arquivos:

| Arquivo | Descrição |
|---------|-----------|
| `StoreSyncBack.Tests/StoreSyncBack.Tests.csproj` | Projeto de testes |
| `StoreSyncBack.Tests/Unit/Services/UserServiceTests.cs` | Testes de UserService |
| `StoreSyncBack.Tests/Unit/Services/CategoryServiceTests.cs` | Testes de CategoryService |
| `StoreSyncBack.Tests/Unit/Services/*ServiceTests.cs` | Demais services |
| `StoreSyncBack.Tests/Fixtures/TestData.cs` | Dados de teste fake |
| `StoreSyncBack.Tests/Helpers/MockHelpers.cs` | Helpers para mocks |

### Arquivos a Modificar:

| Arquivo | Alteração |
|---------|-----------|
| `StoreSyncBack/StoreSyncBack.sln` | Adicionar projeto de testes |
| `CLAUDE.md` | Adicionar seção TDD |

---

## Comandos Úteis

```bash
# Criar projeto de testes
dotnet new xunit -n StoreSyncBack.Tests -o StoreSyncBack.Tests

# Adicionar ao solution
dotnet sln StoreSyncBack/StoreSyncBack.sln add StoreSyncBack.Tests/StoreSyncBack.Tests.csproj

# Executar testes
dotnet test StoreSyncBack.Tests/StoreSyncBack.Tests.csproj

# Executar com detalhes
dotnet test StoreSyncBack.Tests/StoreSyncBack.Tests.csproj --logger "console;verbosity=detailed"

# Executar teste específico
dotnet test --filter "FullyQualifiedName~UserServiceTests"

# Cobertura de código
dotnet test --collect:"XPlat Code Coverage"
```
