using Microsoft.Extensions.Logging;

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
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Add logging to track initialization
        var app = builder.Build();
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("StartupLogger");

        logger.LogInformation("MauiApp initialization started.");

        try
        {
            logger.LogInformation("Configuring services and fonts.");
            // Additional configuration steps can be logged here if needed
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during MauiApp initialization.");
            throw;
        }

        logger.LogInformation("MauiApp initialized successfully.");

        return app;
    }
}
