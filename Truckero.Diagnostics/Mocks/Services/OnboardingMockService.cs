using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Interfaces.Services;

namespace Truckero.Diagnostics.Mocks.Services;

public class OnboardingMockService : IOnboardingService
{
    public async Task StartAsync(StartOnboardingRequest request, Guid userId)
    {
        // TODO: Send verification code (SMS/email/etc.)
        await Task.CompletedTask;
    }

    public async Task<bool> VerifyCodeAsync(VerifyCodeRequest request, Guid userId)
    {
        // TODO: Match verification code and mark as verified
        return await Task.FromResult(true);
    }

    public async Task<OnboardingProgressResponse> GetProgressAsync(Guid userId)
    {
        // TODO: Pull current onboarding state from DB
        return await Task.FromResult(new OnboardingProgressResponse
        {
            EmailVerified = true,
            PhoneVerified = false,
            Role = "Driver",
            Step = "UploadLicense"
        });
    }
    public async Task CompleteCustomerOnboardingAsync(CustomerProfileRequest request, Guid userId)
    {
        // TODO: Save customer-specific profile info
        await Task.CompletedTask;
    }

    public async Task<OperationResult> CompleteDriverOnboardingAsync(DriverProfileRequest request, Guid userId)
    {
        // TODO: Save driver-specific info, create DriverProfile entity, etc.
        await Task.CompletedTask;
        return OperationResult.Succeeded("Driver onboarding completed successfully");
    }

    public Task CompleteCustomerOnboarding(CustomerOnboardingRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<AuthTokenResponse> CompleteCustomerOnboardingAsync(CustomerOnboardingRequest request)
    {
        throw new NotImplementedException();
    }
}

