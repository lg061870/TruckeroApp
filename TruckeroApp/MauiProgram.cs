using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Services;
using Truckero.Infrastructure.Storage;
using TruckeroApp.Extensions;
using TruckeroApp.Interfaces;
using TruckeroApp.Services;
using TruckeroApp.Services.Media;

namespace TruckeroApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit() 
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
            .AddAuthTokenClient(apiBase)
            .AddMediaClient(apiBase);

        builder.Services.AddToastService();
        builder.Services.AddSingleton<ITokenStorageService, SecureTokenStorageService>();
        builder.Services.AddSingleton<IAuthSessionContext, AuthSessionContextService>();
        builder.Services.AddSingleton<IMediaService, MediaService>();
        builder.Services.AddSingleton<IMediaPicker>(MediaPicker.Default);

#if DEBUG
        builder.Services.AddSingleton<IBlobStorageService, MockBlobStorageService>();
#else
        builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();  
#endif




        return builder.Build();
    }
}
