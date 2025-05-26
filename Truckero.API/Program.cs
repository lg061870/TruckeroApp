using Azure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Truckero.API.TestAuth;
using Truckero.Core.Interfaces;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Repositories;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;
using Truckero.Infrastructure.Storage;
using Truckero.API;
using Truckero.Infrastructure.Services;
using Truckero.Diagnostics.Mocks;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// Load base and environment config
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Load secrets from Key Vault if available
var keyVaultUrl = builder.Configuration["KeyVault:Url"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());
}

// Configure Authentication (Azure AD B2C or test fallback)
var b2cOptions = builder.Configuration.GetSection("AzureAdB2C");

if (!env.IsEnvironment("UnitTesting"))
{
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(options =>
        {
            options.ClientId = b2cOptions["ClientId"];
            options.ClientSecret = b2cOptions["ClientSecret"];
            options.Authority = $"{b2cOptions["Instance"]}{b2cOptions["Domain"]}/{b2cOptions["SignUpSignInPolicyId"]}";
            options.ResponseType = "code";
            options.SaveTokens = true;
        });
}
else
{
    builder.Services.AddAuthentication("Test")
        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
}

builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register EF Core DbContext only if using SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Connection: {connectionString}");

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        sql.CommandTimeout(30);
    });

    var loggerFactory = sp.GetService<ILoggerFactory>();
    if (loggerFactory != null) options.UseLoggerFactory(loggerFactory);

    if (env.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register Repositories
builder.Services.AddAuthTokenRepository();
builder.Services.AddRoleRepository();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IOnboardingProgressRepository, OnboardingProgressRepository>();
builder.Services.AddScoped<IConfirmationTokenRepository, ConfirmationTokenRepository>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddSingleton<IHashService, HashService>();

// Register Email Service per environment
#if DEBUG
builder.Services.AddScoped<IEmailService, DevEmailService>();
#else
builder.Services.AddScoped<IEmailService, EmailService>();
#endif

// Register Azure Confidential Client for Production
#if !DEBUG
builder.Services.AddSingleton<IConfidentialClientApplication>(provider =>
{
    var config = builder.Configuration.GetSection("AzureAdB2C");
    return ConfidentialClientApplicationBuilder.Create(config["ClientId"])
        .WithClientSecret(config["ClientSecret"])
        .WithAuthority($"{config["Instance"]}{config["Domain"]}/{config["SignUpSignInPolicyId"]}")
        .Build();
});
#endif



// Swagger (Dev/Test only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Truckero.API", Version = "v1" });
    c.CustomSchemaIds(t => t.FullName);
});

// MVC
builder.Services.AddControllers();

// Logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);

var app = builder.Build();

// Enable Swagger in Dev/Test only
if (env.IsDevelopment() || env.IsEnvironment("UnitTesting"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS before auth
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
