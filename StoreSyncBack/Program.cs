using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;
using Npgsql;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);


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

// Validators (FluentValidation) -- se os validators estiverem em SharedModels
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

services.AddScoped<IUserRepository, StoreSyncBack.Repositories.UserRepository>();
services.AddScoped<IUserService, StoreSyncBack.Services.UserService>();

// Migration Service
services.AddScoped<IMigrationService, MigrationService>();

 var loggerFactory = LoggerFactory.Create(config => config.AddConsole());
var logger = loggerFactory.CreateLogger("Startup");
    
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
            RoleClaimType = ClaimTypes.Role // 🔹 garante que o middleware reconheça o "role"
        };
    });

    services.AddAuthorization();
}
else
{
    logger.LogWarning("JWT key is not configured. Authentication will be disabled.");
}

var app = builder.Build();

// Verifica se o banco de dados está acessível antes de iniciar a API
try
{
    using var scope = app.Services.CreateScope();
    var dbConnection = scope.ServiceProvider.GetRequiredService<System.Data.IDbConnection>();
    dbConnection.Open();
    logger.LogInformation("Conexão com o banco de dados estabelecida com sucesso.");

    // Executa migrations automaticamente na inicialização
    var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationService>();
    await migrationService.ApplyMigrationsAsync();
}
catch (Exception ex)
{
    logger.LogError(ex, "Não foi possível conectar ao banco de dados ou aplicar migrations. A API não será iniciada.");
    Environment.Exit(1);
}

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
logger.LogInformation("Application starting up - environment: {env}", app.Environment.EnvironmentName);


app.MapControllers();
app.Run();
