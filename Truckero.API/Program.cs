using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Truckero.API.TestAuth;
using Truckero.Core.Interfaces;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Repositories;
using Truckero.Infrastructure.Services.Auth;
using Truckero.Infrastructure.Services.Onboarding;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// 📦 Load base and environment config
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 🔐 Load Key Vault (optional)
var keyVaultUrl = builder.Configuration["KeyVault:Url"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());
}

// 🔐 Authentication / Authorization Setup
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

// 🌐 Enable CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 🛢️ Register DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
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

// 🧠 Services & Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthTokenRepository, AuthTokenRepository>();
builder.Services.AddScoped<IAuthService, AuthMockService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();


// 📖 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Truckero.API", Version = "v1" });
    c.CustomSchemaIds(t => t.FullName); // Avoid name collisions
});

// 📡 MVC
builder.Services.AddControllers();

// 🪵 Logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// 🏁 Print env info
Console.WriteLine($"ENV: {env.EnvironmentName}");
Console.WriteLine($"DB: {connectionString}");

var app = builder.Build();

// 🔧 Initialize DB (if needed)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InitializeAsync(db);
}

// 🧪 Swagger only in dev/test
if (env.IsDevelopment() || env.IsEnvironment("UnitTesting"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🌍 CORS before auth
app.UseCors();

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 🔁 Sample endpoint
app.MapGet("/weatherforecast", () =>
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    var forecast = Enumerable.Range(1, 5).Select(i =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )).ToArray();

    return forecast;
})
.RequireAuthorization()
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
