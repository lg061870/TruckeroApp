using Microsoft.Extensions.DependencyInjection;
using System; // Added for Uri and TimeSpan
using System.Net.Http;
using Truckero.Core.Interfaces; // Assuming IAuthTokenRepository is here
using Truckero.Core.Interfaces.Services;
using TruckeroApp.ServiceClients;
// Ensure TruckeroApp.ServiceClients.Mock is present if used, or remove if not
// using TruckeroApp.ServiceClients.Mock; 

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
        services.AddHttpClient<IPaymentService, PaymentApiClientService>(c => // Assuming IPaymentService and PaymentApiClientService exist
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10); // Example timeout
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
        services.AddHttpClient<IPaymentService, PaymentApiClientService>(c => // Assuming IPaymentService and PaymentApiClientService exist
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10); // Example timeout
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

    public static IServiceCollection AddTruckClient(this IServiceCollection services, string apiBase)
    {
#if ANDROID
        services.AddHttpClient<ITruckService, TruckApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10);
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
        services.AddHttpClient<ITruckService, TruckApiClientService>(c =>
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
#if ANDROID
    services.AddHttpClient<IAuthService, AuthApiClientService>(client =>
    {
        client.BaseAddress = new Uri(apiBase);
        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler); // Use CreateUnsafeHandler for consistency
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
#if ANDROID
    services.AddHttpClient<IAuthTokenRepository, AuthTokenApiClientService>(client => // IAuthTokenRepository might be from Core
    {
        client.BaseAddress = new Uri(apiBase);
        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler); // Use CreateUnsafeHandler for consistency
#else
    services.AddHttpClient<IAuthTokenRepository, AuthTokenApiClientService>(client => // IAuthTokenRepository might be from Core
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
        services.AddHttpClient<MediaApiClientService>(c => // Assuming MediaApiClientService doesn't implement a specific core interface directly
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(30);
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
        services.AddHttpClient<MediaApiClientService>(c => // Assuming MediaApiClientService doesn't implement a specific core interface directly
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(30);
        });
#endif
        return services;
    }

    // New methods for PayoutAccount and PaymentMethod
    public static IServiceCollection AddPayoutAccountClient(this IServiceCollection services, string apiBase)
    {
#if ANDROID
        services.AddHttpClient<IPayoutAccountService, PayoutAccountApiClientService>(client =>
        {
            client.BaseAddress = new Uri(apiBase);
            client.Timeout = TimeSpan.FromSeconds(10); // Standard timeout
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
        services.AddHttpClient<IPayoutAccountService, PayoutAccountApiClientService>(client =>
        {
            client.BaseAddress = new Uri(apiBase);
            client.Timeout = TimeSpan.FromSeconds(10); // Standard timeout
        });
#endif
        return services;
    }

    public static IServiceCollection AddPaymentMethodClient(this IServiceCollection services, string apiBase)
    {
#if ANDROID
        services.AddHttpClient<IPaymentMethodService, PaymentMethodApiClientService>(client =>
        {
            client.BaseAddress = new Uri(apiBase);
            client.Timeout = TimeSpan.FromSeconds(10); // Standard timeout
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
        services.AddHttpClient<IPaymentMethodService, PaymentMethodApiClientService>(client =>
        {
            client.BaseAddress = new Uri(apiBase);
            client.Timeout = TimeSpan.FromSeconds(10); // Standard timeout
        });
#endif
        return services;
    }

}
