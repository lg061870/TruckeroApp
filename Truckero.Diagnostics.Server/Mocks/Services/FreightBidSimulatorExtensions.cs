using Microsoft.Extensions.DependencyInjection;
using System;

namespace Truckero.Diagnostics.Mocks.Services
{
    /// <summary>
    /// Extension methods for registering the FreightBidSimulator with the service collection
    /// </summary>
    public static class FreightBidSimulatorExtensions
    {
        /// <summary>
        /// Adds the FreightBidSimulator services to the service collection
        /// </summary>
        public static IServiceCollection AddFreightBidSimulator(this IServiceCollection services)
        {
            // Register the controller
            services.AddMvcCore()
                .AddApplicationPart(typeof(FreightBidSimulatorController).Assembly)
                .AddControllersAsServices();
            
            return services;
        }
    }
}