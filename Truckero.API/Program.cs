using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// 🔐 Connect to Azure Key Vault (Dev)
var secretClient = new SecretClient(
    new Uri("https://truckero-keyvault-dev.vault.azure.net/"),
    new DefaultAzureCredential());

KeyVaultSecret clientIdSecret = secretClient.GetSecret("B2C-Client-ID");
KeyVaultSecret clientSecretSecret = secretClient.GetSecret("B2C-Client-Secret");

// 🪪 Extract secrets
string clientId = clientIdSecret.Value;
string clientSecret = clientSecretSecret.Value;

// 🔐 Add Authentication with Azure AD B2C
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.Authority = $"https://truckeroappauth.b2clogin.com/truckeroappauth.onmicrosoft.com/B2C_1_signupsignin";
        options.ResponseType = "code";
        options.SaveTokens = true;
    });

builder.Services.AddAuthorization();

// 🌐 Add Swagger/OpenAPI (same as before)
builder.Services.AddOpenApi();

var app = builder.Build();

// 🔁 Enable Swagger for Dev only
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 🔐 Auth Middleware
app.UseAuthentication();
app.UseAuthorization();

// 🌤️ Secure Weather Endpoint
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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
