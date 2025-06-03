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
    private readonly IConfirmationTokenRepository _confirmationTokenRepo;
    private readonly IEmailService _emailService;
    private readonly IDriverRepository _driverRepo;

    public OnboardingService(
        ICustomerRepository customerRepo,
        IOnboardingProgressRepository progressRepo,
        IUserRepository userRepo,
        IAuthService authService,
        IHashService hashService,
        AppDbContext dbContext,
        IConfirmationTokenRepository confirmationTokenRepo,
        IEmailService emailService,
        IDriverRepository driverRepo)
    {
        _customerRepo = customerRepo;
        _progressRepo = progressRepo;
        _userRepo = userRepo;
        _authService = authService;
        _hashService = hashService;
        _dbContext = dbContext;
        _confirmationTokenRepo = confirmationTokenRepo;
        _emailService = emailService;
        _driverRepo = driverRepo;
    }

    // --- CompleteCustomerOnboardingAsync ---
    public async Task<AuthTokenResponse> CompleteCustomerOnboardingAsync(CustomerOnboardingRequest request)
    {
        Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var (user, token) = await _authService.RegisterUserAsync(new RegisterUserRequest
                {
                    Email = request.Email,
                    Password = request.Password,
                    Role = "Customer",
                    PhoneNumber = request.PhoneNumber
                });

                var profile = new CustomerProfile
                {
                    UserId = user.Id,
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber ?? string.Empty,
                    Address = request.Address,
                    Email = request.Email
                };

                await _customerRepo.AddCustomerProfileAsync(profile);

                var progress = new OnboardingProgress
                {
                    UserId = user.Id,
                    StepCurrent = "ProfileCreated",
                    Completed = false,
                    LastUpdated = DateTime.UtcNow
                };
                await _progressRepo.AddOrUpdateAsync(progress);

                var confirmationToken = new ConfirmationToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = Guid.NewGuid().ToString(),
                    Used = false,
                    CreatedAt = DateTime.UtcNow,
                    Type = ConfirmationTokenType.EmailConfirmation
                };

                await _confirmationTokenRepo.AddConfirmationTokenAsync(confirmationToken);

                // Perform a single SaveChanges call
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send confirmation email outside transaction
                try
                {
                    await _emailService.SendConfirmationAsync(profile.Email, confirmationToken.Token);
                }
                catch (Exception ex)
                {
                    throw new OnboardingStepException("Failed to send confirmation email.", "failure_send_confirmationemail", ex);
                }

                return new AuthTokenResponse
                {
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken,
                    ExpiresIn = token.ExpiresAt,
                    Success = true,
                    UserId = user.Id
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

    public async Task<AuthTokenResponse> CompleteDriverOnboardingAsync(DriverProfileRequest request)
    {
        Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var (user, token) = await _authService.RegisterUserAsync(new RegisterUserRequest
                {
                    Email = request.Email,
                    Password = request.Password,
                    Role = "Driver",
                    PhoneNumber = request.PhoneNumber,
                    UserId = request.UserId // Ensure UserId is set for the new user
                });

                var driverProfile = new DriverProfile
                {
                    UserId = user.Id,
                    FullName = request.FullName,
                    LicenseNumber = request.LicenseNumber,
                    LicenseFrontUrl = request.LicenseFrontUrl,
                    LicenseBackUrl = request.LicenseBackUrl,
                    HomeBase = request.HomeBase,
                    ServiceRadiusKm = request.ServiceRadiusKm,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Vehicles = request.Trucks?.Select(t => new Truck
                    {
                        Id = Guid.NewGuid(),
                        LicensePlate = t.LicensePlate,
                        Make = t.Make,
                        Model = t.Model,
                        Year = t.Year
                    }).ToList() ?? new List<Truck>()
                };

                await _driverRepo.AddDriverProfileAsync(driverProfile);

                // 4. Track onboarding progress
                var progress = new OnboardingProgress
                {
                    UserId = user.Id,
                    StepCurrent = "ProfileCreated",
                    Completed = false,
                    LastUpdated = DateTime.UtcNow
                };
                await _progressRepo.AddOrUpdateAsync(progress);

                // 5. Generate confirmation token
                var confirmationToken = new ConfirmationToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = Guid.NewGuid().ToString(),
                    Used = false,
                    CreatedAt = DateTime.UtcNow,
                    Type = ConfirmationTokenType.EmailConfirmation
                };

                await _confirmationTokenRepo.AddConfirmationTokenAsync(confirmationToken);

                // 6. Persist all changes in one call
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // 7. Send confirmation email outside the transaction
                try
                {
                    await _emailService.SendConfirmationAsync(request.Email, confirmationToken.Token);
                }
                catch (Exception ex)
                {
                    throw new OnboardingStepException(
                        "Failed to send confirmation email.",
                        "failure_send_confirmationemail",
                        ex
                    );
                }

                // 8. Return auth token
                return new AuthTokenResponse
                {
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken,
                    ExpiresIn = token.ExpiresAt,
                    Success = true,
                    UserId = user.Id
                };
            }
            catch (OnboardingStepException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new OnboardingStepException(
                    "Unexpected onboarding failure.",
                    "unhandled_exception",
                    ex
                );
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

    public Task<OnboardingVerificationResult> VerifyIfOperationSuccessfulAsync(string email)
    {
        // No implementation for this method as the mobile app (TruckeroApp) directly calls
        // the specific API Controller endpoints from its OnboardingApiClientService.
        throw new NotImplementedException();
    }

    public async Task<OperationResult> SendConfirmationEmailAsync(Guid userId)
    {
        // Find user
        var user = await _userRepo.GetUserByIdAsync(userId);
        if (user == null)
            return OperationResult.Failed("User not found.");

        // Generate new confirmation token
        var confirmationToken = new ConfirmationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Guid.NewGuid().ToString(),
            Used = false,
            CreatedAt = DateTime.UtcNow,
            Type = ConfirmationTokenType.EmailConfirmation
        };
        try
        {
            await _confirmationTokenRepo.AddConfirmationTokenAsync(confirmationToken);
            await _confirmationTokenRepo.SaveConfirmationTokenChangesAsync();

            await _emailService.SendConfirmationAsync(user.Email, confirmationToken.Token);

            return OperationResult.Succeeded("Confirmation email sent.");
        }
        catch (Exception ex)
        {
            return OperationResult.Failed($"Failed to send confirmation email: {ex.Message}");
        }
    }

    public async Task<OperationResult> ConfirmEmailAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult.Failed("Invalid or missing confirmation token.");

        var confirmationToken = await _confirmationTokenRepo
            .GetConfirmationTokenByTokenAndTypeAsync(token, ConfirmationTokenType.EmailConfirmation);

        if (confirmationToken == null)
            return OperationResult.Failed("Confirmation token not found or invalid.");

        if (confirmationToken.Used)
            return OperationResult.Failed("This confirmation token has already been used.");

        var user = await _userRepo.GetUserByIdAsync(confirmationToken.UserId);
        if (user == null)
            return OperationResult.Failed("User not found for this confirmation token.");

        // Mark this token as used
        user.EmailVerified = true;
        confirmationToken.Used = true;
        confirmationToken.UsedAt = DateTime.UtcNow;

        // Fetch and mark all other unused confirmation tokens for this user as used
        var otherUnusedTokens = await _confirmationTokenRepo.GetUnusedTokensForUserAsync(
            user.Id,
            ConfirmationTokenType.EmailConfirmation,
            excludeTokenId: confirmationToken.Id);

        await _userRepo.SaveUserChangesAsync();
        await _confirmationTokenRepo.UpdateConfirmationTokenAsync(confirmationToken);
        await _confirmationTokenRepo.SaveConfirmationTokenChangesAsync();

        return OperationResult.Succeeded("Email confirmed successfully.");
    }
    public async Task<List<Truck>> GetDriverTrucksAsync(Guid userId)
    {
        return await _driverRepo.GetVehiclesAsync(userId);
    }

    public async Task<OperationResult> AddDriverTruckAsync(Guid userId, Truck truck)
    {
        try
        {
            // Get the driver profile to validate the user
            var driverProfile = await _driverRepo.GetByUserIdAsync(userId);
            if (driverProfile == null)
                return OperationResult.Failed("Driver profile not found.");

            // Set the driver profile ID
            truck.DriverProfileId = driverProfile.Id;
            
            // Add truck to the repository
            await _driverRepo.AddVehicleAsync(truck);
            await _driverRepo.SaveDriverProfileChangesAsync();
            
            return OperationResult.Succeeded("Truck added successfully.");
        }
        catch (Exception ex)
        {
            return OperationResult.Failed($"Failed to add truck: {ex.Message}");
        }
    }

    public async Task<OperationResult> UpdateDriverTruckAsync(Guid userId, Truck truck)
    {
        try
        {
            // Get the driver profile to validate the user
            var driverProfile = await _driverRepo.GetByUserIdAsync(userId);
            if (driverProfile == null)
                return OperationResult.Failed("Driver profile not found.");
            
            // Get the truck to validate it belongs to this driver
            var existingTruck = await _driverRepo.GetVehicleByIdAsync(truck.Id);
            if (existingTruck == null)
                return OperationResult.Failed("Truck not found.");
                
            if (existingTruck.DriverProfileId != driverProfile.Id)
                return OperationResult.Failed("You don't have permission to update this truck.");
            
            // Update existing truck properties
            existingTruck.LicensePlate = truck.LicensePlate;
            existingTruck.Make = truck.Make;
            existingTruck.Model = truck.Model;
            existingTruck.Year = truck.Year;
            
            // Update in the repository
            await _driverRepo.UpdateVehicleAsync(existingTruck);
            await _driverRepo.SaveDriverProfileChangesAsync();
            
            return OperationResult.Succeeded("Truck updated successfully.");
        }
        catch (Exception ex)
        {
            return OperationResult.Failed($"Failed to update truck: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteDriverTruckAsync(Guid userId, Guid truckId)
    {
        try
        {
            // Get the driver profile to validate the user
            var driverProfile = await _driverRepo.GetByUserIdAsync(userId);
            if (driverProfile == null)
                return OperationResult.Failed("Driver profile not found.");
            
            // Get the truck to validate it belongs to this driver
            var truck = await _driverRepo.GetVehicleByIdAsync(truckId);
            if (truck == null)
                return OperationResult.Failed("Truck not found.");
                
            if (truck.DriverProfileId != driverProfile.Id)
                return OperationResult.Failed("You don't have permission to delete this truck.");
            
            // Delete from the repository
            await _driverRepo.DeleteVehicleAsync(truckId);
            await _driverRepo.SaveDriverProfileChangesAsync();
            
            return OperationResult.Succeeded("Truck deleted successfully.");
        }
        catch (Exception ex)
        {
            return OperationResult.Failed($"Failed to delete truck: {ex.Message}");
        }
    }
}
