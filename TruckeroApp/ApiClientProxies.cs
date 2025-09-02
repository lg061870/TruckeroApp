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

//    public static IServiceCollection AddPayoutAccountClient(this IServiceCollection services, string apiBase) {
//#if ANDROID
//        services.AddHttpClient<IPayoutAccountService, ServiceClients.PayoutAccountApiClientService>(c =>
//        {
//            c.BaseAddress = new Uri(apiBase);
//            c.Timeout = TimeSpan.FromSeconds(10); // Example timeout
//        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
//#else
//    services.AddHttpClient<IPayoutAccountService, ServiceClients.PayoutAccountApiClientService>(c =>
//    {
//        c.BaseAddress = new Uri(apiBase);
//        c.Timeout = TimeSpan.FromSeconds(10); // Example timeout
//    });
//#endif
//        return services;
//    }


    public static IServiceCollection AddCustomerApiClientService(this IServiceCollection services, string apiBase)
    {
#if ANDROID
        services.AddHttpClient<ICustomerProfileService, CustomerApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10);
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
        services.AddHttpClient<ICustomerProfileService, CustomerApiClientService>(c =>
        {
            c.BaseAddress = new Uri(apiBase);
            c.Timeout = TimeSpan.FromSeconds(10);
        });
#endif
        return services;
    }

    public static IServiceCollection AddDriverApiClientService(this IServiceCollection services, string apiBase)
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

    public static IServiceCollection AddUserApiClientService(this IServiceCollection services, string apiBase)
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

    public static IServiceCollection AddTruckApiClientService(this IServiceCollection services, string apiBase)
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

    public static IServiceCollection AddOnboardingApiClientService(this IServiceCollection services, string apiBase)
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

    public static IServiceCollection AddAuthApiClientService(this IServiceCollection services, string apiBase)
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

    public static IServiceCollection AddAuthTokenApiClientService(this IServiceCollection services, string apiBase)
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

    public static IServiceCollection AddMediaApiClientService(this IServiceCollection services, string apiBase)
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
    public static IServiceCollection AddPayoutAccountApiClientService(this IServiceCollection services, string apiBase)
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

    public static IServiceCollection AddPaymentAccountApiClientService(this IServiceCollection services, string apiBase) {
#if ANDROID
        services.AddHttpClient<IPaymentAccountService, PaymentAccountApiClientService>(client => {
            client.BaseAddress = new Uri(apiBase);
            client.Timeout = TimeSpan.FromSeconds(10); // Standard timeout
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
        services.AddHttpClient<IPaymentAccountService, PaymentAccountApiClientService>(client =>
        {
            client.BaseAddress = new Uri(apiBase);
            client.Timeout = TimeSpan.FromSeconds(10); // Standard timeout
        });
#endif
        return services;
    }

    public static IServiceCollection AddPaymentMethodTypeApiClientService(this IServiceCollection services, string apiBase)
    {
#if ANDROID
        services.AddHttpClient<IPaymentMethodTypeService, PaymentMethodTypeApiClientService>(client =>
        {
            client.BaseAddress = new Uri(apiBase);
            client.Timeout = TimeSpan.FromSeconds(10); // Standard timeout
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
        services.AddHttpClient<IPaymentMethodTypeService, PaymentMethodTypeApiClientService>(client =>
        {
            client.BaseAddress = new Uri(apiBase);
            client.Timeout = TimeSpan.FromSeconds(10); // Standard timeout
        });
#endif
        return services;
    }

    public static IServiceCollection AddViewProviderApiClientService(this IServiceCollection services, string apiBase) {
#if ANDROID
        services.AddHttpClient<IViewProviderApiClientService, ViewProviderApiClientService>(client => {
            client.BaseAddress = new Uri(apiBase);
            client.Timeout = TimeSpan.FromSeconds(10);
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
        services.AddHttpClient<IViewProviderApiClientService, ViewProviderApiClientService>(client =>
        {
            client.BaseAddress = new Uri(apiBase);
            client.Timeout = TimeSpan.FromSeconds(10);
        });
#endif
        return services;
    }

    public static IServiceCollection AddCustomerFlowApiClientService(this IServiceCollection services, string apiBase) {
#if ANDROID
        services.AddHttpClient<ICustomerFlowApiClientService, CustomerFlowApiClientService>(client =>
        {
            client.BaseAddress = new Uri(apiBase);
            client.Timeout = TimeSpan.FromSeconds(10);
        }).ConfigurePrimaryHttpMessageHandler(CreateUnsafeHandler);
#else
    services.AddHttpClient<ICustomerFlowApiClientService, CustomerFlowApiClientService>(client =>
    {
        client.BaseAddress = new Uri(apiBase);
        client.Timeout = TimeSpan.FromSeconds(10);
    });
#endif
        return services;
    }


}
