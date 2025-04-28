using Microsoft.Extensions.Logging;

namespace TruckeroApp
{
    public partial class MainPage : ContentPage
    {
        private readonly ILogger<MainPage> _logger;

        public MainPage(ILogger<MainPage> logger)
        {
            _logger = logger;
            _logger.LogInformation("Initializing MainPage...");

            try
            {
                InitializeComponent();
                _logger.LogInformation("MainPage components initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during MainPage initialization.");
                throw;
            }
        }
    }
}
