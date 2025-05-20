using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Onboarding;

namespace Truckero.Core.Interfaces.Services;

public interface IOnboardingService
{
    Task StartAsync(StartOnboardingRequest request, Guid userId);
    Task<bool> VerifyCodeAsync(VerifyCodeRequest request, Guid userId);
    Task<OnboardingProgressResponse> GetProgressAsync(Guid userId);

    Task<OperationResult> CompleteDriverOnboardingAsync(DriverProfileRequest request, Guid userId);
    Task<AuthTokenResponse> CompleteCustomerOnboardingAsync(CustomerOnboardingRequest request);

}
