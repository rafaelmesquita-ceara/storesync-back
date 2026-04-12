using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;
using StoreSyncBack.Middleware;
using Npgsql;
using System.Security.Claims;
using Serilog;
using Serilog.Events;
using Serilog.Filters;

// ── Bootstrap logger (usado antes do host estar pronto) ──────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// ── Serilog ──────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) => lc
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()

    // Console: todos os logs
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}  {Message:lj}{NewLine}{Exception}")

    // Arquivo de logs da aplicação (Warning+) — erros internos, avisos, startup
    .WriteTo.File(
        path: "logs/app-.log",
        restrictedToMinimumLevel: LogEventLevel.Warning,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")

    // Arquivo de requests/responses (sub-logger filtrado por source) — todos os levels
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(
            Matching.FromSource<RequestResponseLoggingMiddleware>())
        .WriteTo.File(
            path: "logs/requests-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}]{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}"))
);

// Bind Jwt settings
services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

// Add controllers + swagger
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StoreSyncBack API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT como: Bearer {token}"
    };

    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});

// Database connection (scoped)
services.AddScoped<System.Data.IDbConnection>(sp =>
{
    var connStr = sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");
    return new NpgsqlConnection(connStr);
});

// Validators (FluentValidation)
services.AddTransient<IValidator<UserLoginDto>, UserLoginDtoValidator>();
services.AddTransient<IValidator<UserChangePasswordDto>, UserChangePasswordDtoValidator>();

// Repositories & Services registrations
services.AddScoped<ICategoryRepository, StoreSyncBack.Repositories.CategoryRepository>();
services.AddScoped<ICategoryService, StoreSyncBack.Services.CategoryService>();

services.AddScoped<IEmployeeRepository, StoreSyncBack.Repositories.EmployeeRepository>();
services.AddScoped<IEmployeeService, StoreSyncBack.Services.EmployeeService>();

services.AddScoped<IFinanceRepository, StoreSyncBack.Repositories.FinanceRepository>();
services.AddScoped<IFinanceService, StoreSyncBack.Services.FinanceService>();

services.AddScoped<IProductRepository, StoreSyncBack.Repositories.ProductRepository>();
services.AddScoped<IProductService, StoreSyncBack.Services.ProductService>();

services.AddScoped<ICommissionRepository, StoreSyncBack.Repositories.CommissionRepository>();
services.AddScoped<ICommissionService, StoreSyncBack.Services.CommissionService>();

services.AddScoped<ISaleItemRepository, StoreSyncBack.Repositories.SaleItemRepository>();
services.AddScoped<ISaleItemService, StoreSyncBack.Services.SaleItemService>();

services.AddScoped<ISaleRepository, StoreSyncBack.Repositories.SaleRepository>();
services.AddScoped<ISaleService, StoreSyncBack.Services.SaleService>();
services.AddScoped<StoreSyncBack.Services.SalesPdfReportService>();

services.AddScoped<IClientRepository, StoreSyncBack.Repositories.ClientRepository>();
services.AddScoped<IClientService, StoreSyncBack.Services.ClientService>();

services.AddScoped<IUserRepository, StoreSyncBack.Repositories.UserRepository>();
services.AddScoped<IUserService, StoreSyncBack.Services.UserService>();

// Migration Service
services.AddScoped<IMigrationService, MigrationService>();

// JWT authentication
var jwtSection = configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key");

if (!string.IsNullOrEmpty(jwtKey))
{
    var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection.GetValue<string>("Issuer"),
            ValidAudience = jwtSection.GetValue<string>("Audience"),
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            RoleClaimType = ClaimTypes.Role
        };
    });

    services.AddAuthorization();
}
else
{
    Log.Warning("JWT key is not configured. Authentication will be disabled.");
}

var app = builder.Build();

// ── Verificação de banco e migrations ────────────────────────────────────────
try
{
    using var scope = app.Services.CreateScope();
    var dbConnection = scope.ServiceProvider.GetRequiredService<System.Data.IDbConnection>();
    dbConnection.Open();
    Log.Information("Conexão com o banco de dados estabelecida com sucesso.");

    var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationService>();
    await migrationService.ApplyMigrationsAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Não foi possível conectar ao banco de dados ou aplicar migrations. A API não será iniciada.");
    await Log.CloseAndFlushAsync();
    Environment.Exit(1);
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
// Logging primeiro para capturar todas as requests
app.UseMiddleware<RequestResponseLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "StoreSyncBack v1"));
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

if (!string.IsNullOrEmpty(jwtKey))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

Log.Information("API iniciando — ambiente: {Env}", app.Environment.EnvironmentName);

app.MapControllers().RequireAuthorization();
app.Run();
