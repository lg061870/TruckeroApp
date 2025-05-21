using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;
using Truckero.Infrastructure.Data;


namespace Truckero.Infrastructure.Services;

public class OnboardingService : IOnboardingService
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IOnboardingProgressRepository _progressRepo;
    private readonly IUserRepository _userRepo;
    private readonly IAuthService _authService;
    private readonly IHashService _hashService;
    private readonly AppDbContext _dbContext;


    public OnboardingService(
        ICustomerRepository customerRepo,
        IOnboardingProgressRepository progressRepo,
        IUserRepository userRepo,
        IAuthService authService,
        IHashService hashService,
        AppDbContext dbContext)
    {
        _customerRepo = customerRepo;
        _progressRepo = progressRepo;
        _userRepo = userRepo;
        _authService = authService;
        _hashService = hashService;
        _dbContext = dbContext;
    }

    public async Task<AuthTokenResponse> CompleteCustomerOnboardingAsync(CustomerOnboardingRequest request)
    {
        Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var registerResult = await _authService.RegisterAsync(new RegisterUserRequest
                {
                    Email = request.Email,
                    Password = request.Password,
                    Role = "Customer",
                    PhoneNumber = request.PhoneNumber
                });

                if (string.IsNullOrWhiteSpace(registerResult.AccessToken) || string.IsNullOrWhiteSpace(registerResult.RefreshToken))
                    throw new OnboardingStepException("Registration failed: token not generated.", "auth_register_failed");

                var profile = new CustomerProfile
                {
                    UserId = registerResult.UserId,
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber ?? string.Empty,
                    Address = request.Address,
                    Email = request.Email
                };

                try
                {
                    await _customerRepo.AddAsync(profile);
                    await _customerRepo.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new OnboardingStepException("Failed to create customer profile.", "profile_creation_failed", ex);
                }

                try
                {
                    var progress = new OnboardingProgress
                    {
                        UserId = registerResult.UserId,
                        StepCurrent = "ProfileCreated",
                        Completed = false,
                        LastUpdated = DateTime.UtcNow
                    };

                    await _progressRepo.AddOrUpdateAsync(progress);
                    await _progressRepo.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new OnboardingStepException("Failed to save onboarding progress.", "progress_save_failed", ex);
                }

                await transaction.CommitAsync();

                return new AuthTokenResponse
                {
                    AccessToken = registerResult.AccessToken,
                    RefreshToken = registerResult.RefreshToken,
                    ExpiresIn = registerResult.ExpiresIn,
                    Success = true,
                    UserId = registerResult.UserId
                };
            }
            catch (OnboardingStepException oex)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new OnboardingStepException("Unexpected onboarding failure.", "unhandled_exception", ex);
            }
        });
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
