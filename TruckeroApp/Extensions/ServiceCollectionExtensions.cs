using Microsoft.Extensions.DependencyInjection;
using System;
using Truckero.Core.Interfaces.Services;
using TruckeroApp.ServiceClients;
using TruckeroApp.Services;
using TruckeroApp.Services.Media;

namespace TruckeroApp.Extensions
{
    /// <summary>
    /// Extension methods for registering services in the DI container
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the media services to the service collection
        /// </summary>
        public static IServiceCollection AddMediaServices(this IServiceCollection services)
        {
            services.AddScoped<IMediaService, MediaService>();
            
            // Register the MediaApiClientService
            services.AddHttpClient<MediaApiClientService>((sp, client) => {
                // Base URL is configured when the client is created in MauiProgram.cs
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            
            return services;
        }

        /// <summary>
        /// Adds the toast notification service to the service collection
        /// </summary>
        public static IServiceCollection AddToastService(this IServiceCollection services)
        {
            services.AddScoped<IToastService, ToastService>();
            return services;
        }
    }
}
