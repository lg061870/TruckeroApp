using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Services;

public interface IOnboardingService
{
    Task StartAsync(StartOnboardingRequest request, Guid userId);
    Task<bool> VerifyCodeAsync(VerifyCodeRequest request, Guid userId);
    Task<OnboardingProgressResponse> GetProgressAsync(Guid userId);
    Task<AuthTokenResponse> CompleteDriverOnboardingAsync(DriverProfileRequest request);
    Task<AuthTokenResponse> CompleteCustomerOnboardingAsync(CustomerOnboardingRequest request);
    Task<OnboardingVerificationResult> VerifyIfOperationSuccessfulAsync(string email);
    Task<OperationResult> SendConfirmationEmailAsync(Guid userId);
    Task<OperationResult> ConfirmEmailAsync(string token);
    
    // Truck management methods
    Task<List<Truck>> GetDriverTrucksAsync(Guid userId);
    Task<OperationResult> AddDriverTruckAsync(Guid userId, Truck truck);
    Task<OperationResult> UpdateDriverTruckAsync(Guid userId, Truck truck);
    Task<OperationResult> DeleteDriverTruckAsync(Guid userId, Guid truckId);
}
