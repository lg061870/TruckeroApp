using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Onboarding;

namespace Truckero.Core.Interfaces;

public interface IOnboardingService
{
    Task StartAsync(StartOnboardingRequest request, Guid userId);
    Task<bool> VerifyCodeAsync(VerifyCodeRequest request, Guid userId);
    Task<OnboardingProgressResponse> GetProgressAsync(Guid userId);

    Task CompleteCustomerOnboardingAsync(CustomerProfileRequest request, Guid userId);
    Task CompleteDriverOnboardingAsync(DriverProfileRequest request, Guid userId);
}
