using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Interfaces.Services;

namespace Truckero.API.Tests.Mocks;

public class OnboardingMockService : IOnboardingService
{
    private readonly Guid _mockUserId = Guid.NewGuid();

    public Task StartAsync(StartOnboardingRequest request, Guid userId)
    {
        return Task.CompletedTask;
    }

    public Task<bool> VerifyCodeAsync(VerifyCodeRequest request, Guid userId)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(request.Code) && request.UserId != Guid.Empty);
    }

    public Task<OnboardingProgressResponse> GetProgressAsync(Guid userId)
    {
        return Task.FromResult(new OnboardingProgressResponse
        {
            Role = "Customer",
            Step = "PaymentMethod",
            EmailVerified = true,
            PhoneVerified = true
        });
    }

    public Task CompleteCustomerOnboardingAsync(CustomerProfileRequest request, Guid userId)
    {
        return Task.CompletedTask;
    }

    public Task<OperationResult> CompleteDriverOnboardingAsync(DriverProfileRequest request, Guid userId)
    {
        return Task.FromResult(OperationResult.Succeeded("Driver onboarding completed successfully"));
    }

    public Task CompleteCustomerOnboarding(CustomerOnboardingRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<AuthTokenResponse> CompleteCustomerOnboardingAsync(CustomerOnboardingRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<OnboardingVerificationResult> VerifyIfOperationSuccessfulAsync(string email)
    {
        // Return a mock result for testing
        return Task.FromResult(new OnboardingVerificationResult
        {
            UserFound = true,
            ProfileFound = true,
            TokenFound = true,
            UserId = _mockUserId
        });
    }

    public Task<OperationResult> SendConfirmationEmailAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<OperationResult> ConfirmEmailAsync(string token)
    {
        throw new NotImplementedException();
    }
}
