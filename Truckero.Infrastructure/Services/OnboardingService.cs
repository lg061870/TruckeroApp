using Truckero.Core.DTOs.Auth;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;


namespace Truckero.Infrastructure.Services;

public class OnboardingService : IOnboardingService
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IOnboardingProgressRepository _progressRepo;
    private readonly IUserRepository _userRepo;
    private readonly IAuthService _authService;
    private readonly IHashService _hashService;


    public OnboardingService(
        ICustomerRepository customerRepo,
        IOnboardingProgressRepository progressRepo,
        IUserRepository userRepo,
        IAuthService authService,
        IHashService hashService)
    {
        _customerRepo = customerRepo;
        _progressRepo = progressRepo;
        _userRepo = userRepo;
        _authService = authService;
        _hashService = hashService;
    }

    public async Task<AuthTokenResponse> CompleteCustomerOnboardingAsync(CustomerOnboardingRequest request)
    {
        // 1. Register user via AuthService
        var registerResult = await _authService.RegisterAsync(new RegisterUserRequest
        {
            Email = request.Email,
            Password = request.Password,
            Role = "Customer"
        });

        if (!registerResult.Success)
            throw new InvalidOperationException(registerResult.ErrorMessage ?? "Registration failed.");

        var userId = registerResult.UserId;

        // 2. Create customer profile
        var profile = new CustomerProfile
        {
            UserId = userId,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber ?? string.Empty,
            Address = request.Address
        };

        await _customerRepo.AddAsync(profile);
        await _customerRepo.SaveChangesAsync();

        // 3. Save onboarding progress
        var progress = new OnboardingProgress
        {
            UserId = userId,
            StepCurrent = "ProfileCreated",
            Completed = false,
            LastUpdated = DateTime.UtcNow
        };

        await _progressRepo.AddOrUpdateAsync(progress);
        await _progressRepo.SaveChangesAsync();

        // 4. Return the auth token
        return new AuthTokenResponse
        {
            AccessToken = registerResult.AccessToken,
            RefreshToken = registerResult.RefreshToken,
            ExpiresIn = registerResult.ExpiresIn
        };
    }

    public Task StartAsync(StartOnboardingRequest request, Guid userId)
    {
        // TODO: Implement verification (e.g., via SMS/email provider)
        throw new NotImplementedException();
    }

    public Task<bool> VerifyCodeAsync(VerifyCodeRequest request, Guid userId)
    {
        // TODO: Validate code from provider or stored table
        throw new NotImplementedException();
    }

    public async Task<OnboardingProgressResponse> GetProgressAsync(Guid userId)
    {
        var progress = await _progressRepo.GetByUserIdAsync(userId);
        return progress is null
            ? new OnboardingProgressResponse { StepCurrent = "NotStarted", Completed = false }
            : new OnboardingProgressResponse
            {
                StepCurrent = progress.StepCurrent,
                Completed = progress.Completed,
                LastUpdated = progress.LastUpdated
            };
    }

    public Task<OperationResult> CompleteDriverOnboardingAsync(DriverProfileRequest request, Guid userId)
    {
        // TODO: Wire up driver onboarding once driver repo is ready
        // For now, return a placeholder result
        return Task.FromResult(OperationResult.Succeeded("Driver onboarding completed successfully"));
    }
}
