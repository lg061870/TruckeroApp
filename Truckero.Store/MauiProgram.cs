using Microsoft.Extensions.Logging;
using Truckero.Store.ServiceClients;
using Truckero.Store.Services;

namespace Truckero.Store;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

#if ANDROID
        var apiBase = "http://10.0.2.2:5200";
#elif IOS
        var apiBase = "http://localhost:5200";
#else
        var apiBase = "http://localhost:5200";
#endif

        builder.Services.AddSingleton(_ =>
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiBase),
                Timeout = TimeSpan.FromSeconds(20)
            };

            return new StoreApiClient(httpClient);
        });

        builder.Services.AddSingleton<StoreSessionState>();

        return builder.Build();
    }
}
