# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

StoreSyncBack is an ASP.NET Core 9.0 Web API for store management. It uses PostgreSQL for data storage with Dapper as the micro-ORM.

## Project Structure

- **StoreSyncBack/** - Main ASP.NET Web API project containing controllers, services, repositories
- **SharedModels/** - Shared library containing domain models, interfaces, and FluentValidation validators
- **StoreSyncBack/BD/** - PostgreSQL database scripts

## Architecture

The codebase follows the Repository Pattern with a Service Layer:

```
Controllers -> Services -> Repositories -> Database (PostgreSQL via Dapper)
```

- **Controllers** (`StoreSyncBack/Controllers/`) - API endpoints, handle HTTP requests/responses
- **Services** (`StoreSyncBack/Services/`) - Business logic, validation, JWT token generation
- **Repositories** (`StoreSyncBack/Repositories/`) - Data access using Dapper, SQL queries
- **Models/Interfaces** (`SharedModels/`) - Domain models and interface contracts

Each domain entity (User, Employee, Category, Product, Sale, SaleItem, Commission, Finance) has its own Controller, Service, Repository, Model, and Interface.

## Technology Stack

- **.NET 9.0** - Target framework
- **PostgreSQL** - Database (Npgsql driver)
- **Dapper** - Micro-ORM for SQL queries
- **JWT Authentication** - Token-based auth (Microsoft.AspNetCore.Authentication.JwtBearer)
- **FluentValidation** - Input validation
- **BCrypt** - Password hashing
- **Swagger** - API documentation

## Common Commands

### Build and Run

```bash
# Build the solution
dotnet build StoreSyncBack/StoreSyncBack.sln

# Run the API (development)
dotnet run --project StoreSyncBack/StoreSyncBack.csproj

# Run with HTTPS profile
dotnet run --project StoreSyncBack/StoreSyncBack.csproj --launch-profile https
```

The API runs on:
- HTTP: http://localhost:5269
- HTTPS: https://localhost:7044 (when using https profile)
- Swagger UI: http://localhost:5269/swagger (Development mode)

### Database Configuration

Connection string is configured in `StoreSyncBack/appsettings.json`:

```json
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=storesync;Username=store;Password=storepass"
}
```

### Authentication

JWT settings are in `appsettings.json`. The default key is for development only. The system supports role-based authorization using `Employee.Role` (e.g., "admin").

### Default Admin User

Run `StoreSyncBack/BD/inserir_root.sql` to create an admin user:
- Login: `admin`
- Password: `admin` (hashed with BCrypt)

## Domain Entities

All entities use `Guid` for primary keys:

- **User** - Authentication, linked to Employee
- **Employee** - Staff members with roles (admin, etc.) and commission rates
- **Category** - Product categories
- **Product** - Store products
- **Sale** - Sales transactions
- **SaleItem** - Individual items within a sale
- **Commission** - Employee commissions
- **Finance** - Financial records

## Key Implementation Patterns

### Repository Pattern with Dapper

Repositories use Dapper for raw SQL queries with mapped results:

```csharp
public async Task<User?> GetUserByIdAsync(Guid userId)
{
    var sql = @"SELECT user_id AS UserId, login AS Login...
                FROM ""user""
                WHERE user_id = @Id;";
    return await _db.QueryFirstOrDefaultAsync<User?>(sql, new { Id = userId });
}
```

Note: PostgreSQL table `"user"` uses double quotes as it's a reserved keyword.

### Validation

Validators are defined in SharedModels using FluentValidation:

```csharp
public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(dto => dto.Login).NotEmpty();
        RuleFor(dto => dto.Password).NotEmpty();
    }
}
```

### Dependency Injection

Services and repositories are registered in `Program.cs`:

```csharp
services.AddScoped<IUserRepository, StoreSyncBack.Repositories.UserRepository>();
services.AddScoped<IUserService, StoreSyncBack.Services.UserService>();
services.AddTransient<IValidator<UserLoginDto>, UserLoginDtoValidator>();
```

### Password Security

Passwords are hashed using BCrypt before storage:

```csharp
user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
```

## Notes

- Portuguese is used for log messages and some validation error messages (e.g., "Senha inválida")
- JWT token includes the Employee.Role claim for authorization
- Controllers return password-masked responses (setting `Password = null` before returning)
- The `"user"` table name requires double quotes in PostgreSQL queries due to being a reserved keyword
- Para qualquer alteração solicitada na qual seja solicitado um commit e push, a mensagem do comit deve ser um resumo bem objetivo do que foi feito.

## Database Migrations

The project uses a custom migration system that automatically applies pending SQL scripts on application startup.

### How It Works

1. **Startup Process**: When the API starts, `MigrationService.ApplyMigrationsAsync()` is called automatically
2. **Version Tracking**: Applied migrations are tracked in the `historico_versao` table
3. **Automatic Execution**: Only migrations with version numbers greater than the last applied migration are executed
4. **Transaction Safety**: Each migration runs within a transaction - if it fails, it rolls back

### Migration Files

Location: `StoreSyncBack/Migrations/`

Naming convention: `{version}_{description}.sql`

Examples:
- `000_initial_schema.sql` - Creates all database tables
- `001_seed_root_user.sql` - Inserts default admin user

### Adding a New Migration

1. Create a new SQL file in `StoreSyncBack/Migrations/` following the naming pattern
2. Use sequential version numbers (002, 003, etc.)
3. Write idempotent SQL using `IF NOT EXISTS` where possible
4. Restart the application - migrations are applied automatically

Example:
```sql
-- 002_add_product_description.sql
ALTER TABLE product ADD COLUMN IF NOT EXISTS description TEXT;
```

### Migration History Table

```sql
CREATE TABLE historico_versao (
    id SERIAL PRIMARY KEY,
    numero_release VARCHAR(20) NOT NULL UNIQUE,
    data_atualizacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

Query to check applied migrations:
```sql
SELECT * FROM historico_versao ORDER BY numero_release;
```

## Test-Driven Development (TDD)

Este projeto segue o padrão **TDD (Test-Driven Development)**. Toda nova funcionalidade ou alteração deve seguir o ciclo:

```
Vermelho → Verde → Refatorar
```

### Ciclo TDD

1. **Vermelho**: Escrever o teste ANTES da implementação (o teste deve falhar)
2. **Verde**: Implementar o mínimo necessário para o teste passar
3. **Refatorar**: Melhorar o código mantendo todos os testes verdes

### Regras Obrigatórias

- ✅ Todo código de produção deve ter teste unitário correspondente
- ✅ Testes devem ser escritos ANTES da implementação
- ✅ Executar `dotnet test` antes de cada commit
- ✅ Cobertura mínima: 80% dos services
- ✅ Usar xUnit + Moq + FluentAssertions

### Executando Testes

```bash
# Executar todos os testes
dotnet test StoreSyncBack.Tests/StoreSyncBack.Tests.csproj

# Executar com detalhes
dotnet test StoreSyncBack.Tests/StoreSyncBack.Tests.csproj --logger "console;verbosity=detailed"

# Executar testes específicos
dotnet test --filter "FullyQualifiedName~UserServiceTests"

# Executar em modo watch (desenvolvimento)
dotnet watch test --project StoreSyncBack.Tests/StoreSyncBack.Tests.csproj
```

### Estrutura de Testes

- **Local**: `StoreSyncBack.Tests/Unit/Services/`
- **Padrão de nomenclatura**: `[Metodo]_[Estado]_[ResultadoEsperado]`
- Um arquivo de teste por service
- Usar `TestData` fixture para dados fake
- Mockar repositórios com Moq

### Exemplo de Teste

```csharp
[Fact]
public async Task CreateCategoryAsync_NomeValido_RetornaIdCriado()
{
    // Arrange
    var category = TestData.CreateCategory("Eletrônicos");
    _repoMock.Setup(r => r.CreateCategoryAsync(It.IsAny<Category>()))
        .ReturnsAsync(1);

    // Act
    var result = await _service.CreateCategoryAsync(category);

    // Assert
    result.Should().Be(1);
    _repoMock.Verify(r => r.CreateCategoryAsync(
        It.Is<Category>(c => c.Name == "Eletrônicos")), Times.Once);
}
```

### Fluxo de Trabalho

Ao implementar uma nova funcionalidade:

1. Criar branch para a feature
2. Escrever testes que definem o comportamento esperado
3. Executar testes e confirmar que falham (vermelho)
4. Implementar a funcionalidade mínima
5. Executar testes e confirmar que passam (verde)
6. Refatorar se necessário
7. Commit com mensagem objetiva
8. Push e merge

### Dados de Teste

Usar a classe `TestData` em `StoreSyncBack.Tests/Fixtures/`:

```csharp
var category = TestData.CreateCategory("Nome");
var user = TestData.CreateUser("login", "senha");
var products = TestData.CreateProducts(5);
```
