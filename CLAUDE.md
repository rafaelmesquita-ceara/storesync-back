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
