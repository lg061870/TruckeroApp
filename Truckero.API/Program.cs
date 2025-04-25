using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.EntityFrameworkCore;
using Truckero.Infrastructure.Data;
using Truckero.Core.Interfaces;
using Truckero.Infrastructure.Services.Onboarding;
using Truckero.Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Authentication; // ✅ Add this to use AppDbContext and DbInitializer
using Truckero.API.TestAuth;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// 📦 Load appsettings.{env}.json + secrets
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 🔐 Optional: Load Key Vault based on env
var keyVaultUrl = builder.Configuration["KeyVault:Url"];

if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}

// 🪪 Bind B2C options from final merged config
var b2cOptions = builder.Configuration.GetSection("AzureAdB2C");

if (!builder.Environment.IsEnvironment("UnitTesting"))
{
    // 🔐 Configure Azure AD B2C Auth
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        options.ClientId = b2cOptions["ClientId"];
        options.ClientSecret = b2cOptions["ClientSecret"];
        options.Authority = $"{b2cOptions["Instance"]}{b2cOptions["Domain"]}/{b2cOptions["SignUpSignInPolicyId"]}";
        options.ResponseType = "code";
        options.SaveTokens = true;
    });

    builder.Services.AddAuthorization();
}
else
{
    // 🧪 Inject test auth handler — now correctly referenced inside Truckero.API
    builder.Services.AddAuthentication("Test")
        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

    builder.Services.AddAuthorization();
}

// ✅ Register AppDbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🌐 Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Truckero.API", Version = "v1" });
    c.CustomSchemaIds(type => type.FullName); // Helps avoid name collisions
});


builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

Console.WriteLine("ENV: " + env.EnvironmentName);
Console.WriteLine("DB: " + builder.Configuration.GetConnectionString("DefaultConnection"));

var app = builder.Build();


// ✅ Seed database using DbInitializer
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InitializeAsync(db);
}

// 🔁 Enable Swagger for Dev only
if (env.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ✅ Test endpoint
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.RequireAuthorization()
.WithName("GetWeatherForecast");

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
