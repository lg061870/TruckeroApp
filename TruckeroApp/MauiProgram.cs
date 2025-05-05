using Microsoft.Extensions.Logging;
using TruckeroApp.ServiceClients;
using Microsoft.Extensions.DependencyInjection;
using Truckero.Core.Interfaces;
using System.Net.Http;

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

        // ✅ Platform-specific base URL
#if ANDROID
        var apiBase = "http://10.0.2.2:5200";
#elif IOS
        var apiBase = "http://localhost:5200"; // iOS Simulator
#else
        var apiBase = "https://localhost:7224"; // Windows/macOS
#endif

        // ✅ Configure HttpClient for Payment API
#if ANDROID
        builder.Services.AddHttpClient<IPaymentService, PaymentApiClientService>(client =>
        {
            client.BaseAddress = new Uri(apiBase);
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
#else
        builder.Services.AddHttpClient<IPaymentService, PaymentApiClientService>(client =>
        {
            client.BaseAddress = new Uri(apiBase);
            client.Timeout = TimeSpan.FromSeconds(10);
        });
#endif


        // ✅ DI service abstraction
        //builder.Services.AddScoped<IPaymentService, PaymentApiClientService>();

        return builder.Build();
    }
}
