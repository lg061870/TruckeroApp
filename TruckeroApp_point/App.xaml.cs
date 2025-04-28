using Microsoft.Extensions.Logging;
using Microsoft.Maui;

namespace TruckeroApp
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var services = IPlatformApplication.Current?.Services;
            if (services == null)
            {
                throw new InvalidOperationException("Services are not available.");
            }

            var logger = services.GetRequiredService<ILogger<MainPage>>();
            var mainPage = new MainPage(logger);
            return new Window(mainPage) { Title = "TruckeroApp" };
        }
    }
}
