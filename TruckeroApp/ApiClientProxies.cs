using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Services;
using TruckeroApp.ServiceClients;
using TruckeroApp.ServiceClients.Mock;

namespace TruckeroApp;

public static class ApiClientProxies
{
    private static HttpClientHandler CreateUnsafeHandler() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    public static IServiceCollection AddPaymentClient(this IServiceCollection services, string apiBase)
    {
#if ANDROID
        services.AddHttpClient<IPaymentService, PaymentApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10);
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
        services.AddHttpClient<IPaymentService, PaymentApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10);
        });
#endif
        return services;
    }
    public static IServiceCollection AddCustomerClient(this IServiceCollection services, string apiBase)
    {
#if ANDROID
        services.AddHttpClient<ICustomerService, CustomerApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10);
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
        services.AddHttpClient<ICustomerService, CustomerApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10);
        });
#endif
        return services;
    }
    public static IServiceCollection AddDriverClient(this IServiceCollection services, string apiBase)
    {
#if ANDROID
        services.AddHttpClient<IDriverService, DriverApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10);
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
        services.AddHttpClient<IDriverService, DriverApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10);
        });
#endif
        return services;
    }
    public static IServiceCollection AddUserClient(this IServiceCollection services, string apiBase)
    {
#if ANDROID
        services.AddHttpClient<IUserService, UserApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10);
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
        services.AddHttpClient<IUserService, UserApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10);
        });
#endif
        return services;
    }
    public static IServiceCollection AddVehicleClient(this IServiceCollection services, string apiBase)
    {
#if ANDROID
        services.AddHttpClient<IVehicleService, VehicleApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10);
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
        services.AddHttpClient<IVehicleService, VehicleApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10);
        });
#endif
        return services;
    }
    public static IServiceCollection AddOnboardingClient(this IServiceCollection services, string apiBase)
    {
#if ANDROID
        services.AddHttpClient<IOnboardingService, OnboardingApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10);
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
    services.AddHttpClient<IOnboardingService, OnboardingApiClientService>(c =>
    {
        c.BaseAddress = new Uri(apiBase);
        c.Timeout = TimeSpan.FromSeconds(10);
    });
#endif
        return services;
    }
    public static IServiceCollection AddAuthClient(this IServiceCollection services, string apiBase)
    {
    // 🌐 Use real API HTTP client
#if ANDROID
    services.AddHttpClient<IAuthService, AuthApiClientService>(client =>
    {
        client.BaseAddress = new Uri(apiBase);
        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
#else
    services.AddHttpClient<IAuthService, AuthApiClientService>(client =>
    {
        client.BaseAddress = new Uri(apiBase);
        client.Timeout = TimeSpan.FromSeconds(10);
    });
#endif
        return services;
    }
    public static IServiceCollection AddAuthTokenClient(this IServiceCollection services, string apiBase)
    {
    // Use real HTTP-based client
#if ANDROID
    services.AddHttpClient<IAuthTokenRepository, AuthTokenApiClientService>(client =>
    {
        client.BaseAddress = new Uri(apiBase);
        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
#else
    services.AddHttpClient<IAuthTokenRepository, AuthTokenApiClientService>(client =>
    {
        client.BaseAddress = new Uri(apiBase);
        client.Timeout = TimeSpan.FromSeconds(10);
    });
#endif
        return services;
    }
    public static IServiceCollection AddMediaClient(this IServiceCollection services, string apiBase)
    {
#if ANDROID
        services.AddHttpClient<MediaApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(30);
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
        services.AddHttpClient<MediaApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(30);
        });
#endif
        return services;
    }
}
