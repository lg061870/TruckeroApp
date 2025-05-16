using Microsoft.Extensions.Logging;
using TruckeroApp;
using Microsoft.Extensions.DependencyInjection;
using TruckeroApp.Interfaces;
using TruckeroApp.Session;
using TruckeroApp.Services;
using Truckero.Core.Interfaces;
using Truckero.Infrastructure.Repositories;


namespace TruckeroApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Inter-Regular.ttf", "InterRegular");
                fonts.AddFont("Inter-Bold.ttf", "InterBold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddMauiBlazorWebView();

        // 🌐 Platform-specific API base URL
#if ANDROID
        var apiBase = "http://10.0.2.2:5200";
#elif IOS
        var apiBase = "http://localhost:5200";
#else
        var apiBase = "https://localhost:7224";
#endif

        // 🔌 Modular API client registration
        builder.Services
            .AddPaymentClient(apiBase)
            .AddCustomerClient(apiBase)
            .AddDriverClient(apiBase)
            .AddUserClient(apiBase)
            .AddVehicleClient(apiBase)
            .AddOnboardingClient(apiBase)
            .AddAuthClient(apiBase)
            .AddAuthTokenClient(apiBase);

        builder.Services.AddSingleton<ITokenStorageService, SecureTokenStorageService>();
        builder.Services.AddSingleton<IAuthSessionContext, AuthSessionContextService>();

        return builder.Build();
    }
}
