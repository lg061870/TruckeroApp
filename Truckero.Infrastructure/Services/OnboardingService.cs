using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.DTOs.Trucks;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Services;

public class OnboardingService : IOnboardingService {
    private readonly ICustomerRepository _customerRepo;
    private readonly IOnboardingProgressRepository _progressRepo;
    private readonly IUserRepository _userRepo;
    private readonly IAuthService _authService;
    private readonly IHashService _hashService;
    private readonly AppDbContext _dbContext;
    private readonly IConfirmationTokenRepository _confirmationTokenRepo;
    private readonly IEmailService _emailService;
    private readonly IDriverRepository _driverRepo;
    private readonly IPayoutAccountRepository _payoutAccountRepo;

    public OnboardingService(
        ICustomerRepository customerRepo,
        IOnboardingProgressRepository progressRepo,
        IUserRepository userRepo,
        IAuthService authService,
        IHashService hashService,
        AppDbContext dbContext,
        IConfirmationTokenRepository confirmationTokenRepo,
        IEmailService emailService,
        IDriverRepository driverRepo,
        IPayoutAccountRepository payoutAccountRepo) {
        _customerRepo = customerRepo;
        _progressRepo = progressRepo;
        _userRepo = userRepo;
        _authService = authService;
        _hashService = hashService;
        _dbContext = dbContext;
        _confirmationTokenRepo = confirmationTokenRepo;
        _emailService = emailService;
        _driverRepo = driverRepo;
        _payoutAccountRepo = payoutAccountRepo;
    }

    // --- CompleteCustomerOnboardingAsync ---
    public async Task<AuthTokenResponse> CompleteCustomerOnboardingAsync(CustomerOnboardingRequest request) {
        Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () => {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try {
                var (user, token) = await _authService.RegisterUserAsync(new RegisterUserRequest {
                    Email = request.Email,
                    Password = request.Password,
                    Role = "Customer",
                    PhoneNumber = request.PhoneNumber,
                    UserId = request.UserId,
                    FullName = request.FullName,
                    Address = request.Address,
                });

                var profile = new CustomerProfile {
                    UserId = user.Id,
                    User = user
                };

                await _customerRepo.AddCustomerProfileAsync(profile);

                var progress = new OnboardingProgress {
                    UserId = user.Id,
                    StepCurrent = "ProfileCreated",
                    Completed = false,
                    LastUpdated = DateTime.UtcNow
                };
                await _progressRepo.AddOrUpdateAsync(progress);

                var confirmationToken = new ConfirmationToken {
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
                try {
                    await _emailService.SendConfirmationAsync(request.Email, confirmationToken.Token);
                } catch (Exception ex) {
                    throw new OnboardingStepException("Failed to send confirmation email.", "failure_send_confirmationemail", ex);
                }

                return new AuthTokenResponse {
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken,
                    ExpiresIn = token.ExpiresAt,
                    Success = true,
                    UserId = user.Id
                };
            } catch (OnboardingStepException oex) {
                await transaction.RollbackAsync();
                throw;
            } catch (Exception ex) {
                await transaction.RollbackAsync();
                throw new OnboardingStepException("Unexpected onboarding failure.", "unhandled_exception", ex);
            }
        });
    }

    public async Task<AuthTokenResponse> CompleteDriverOnboardingAsync(DriverProfileRequest request) {
        Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () => {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try {
                var (user, token) = await _authService.RegisterUserAsync(new RegisterUserRequest {
                    Email = request.Email,
                    Password = request.Password,
                    Role = "Driver",
                    PhoneNumber = request.PhoneNumber,
                    UserId = request.UserId,
                    FullName = request.FullName,
                });

                var driverProfile = new DriverProfile {
                    UserId = user.Id,
                    LicenseNumber = request.LicenseNumber,
                    LicenseFrontUrl = request.LicenseFrontUrl,
                    LicenseBackUrl = request.LicenseBackUrl,
                    HomeBase = request.HomeBase,
                    ServiceRadiusKm = request.ServiceRadiusKm,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    User = user,
                    Trucks = request.Trucks?.Select(t => new Truck {
                        Id = Guid.NewGuid(),
                        LicensePlate = t.LicensePlate,
                        TruckMakeId = t.TruckMakeId,
                        TruckTypeId = t.TruckTypeId,
                        TruckModelId = t.TruckModelId,
                        Year = t.Year
                    }).ToList() ?? new List<Truck>()
                };

                await _driverRepo.AddDriverProfileAsync(driverProfile);

                // Save payout accounts if provided
                if (request.PayoutAccounts != null && request.PayoutAccounts.Count > 0) {
                    foreach (var payoutDto in request.PayoutAccounts) {
                        var payout = new PayoutAccount {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            PaymentMethodTypeId = payoutDto.PaymentMethodTypeId,
                            AccountNumberLast4 = payoutDto.AccountNumberLast4,
                            IsDefault = payoutDto.IsDefault,
                            MetadataJson = payoutDto.MetadataJson,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _payoutAccountRepo.AddAsync(payout);
                    }
                }

                var progress = new OnboardingProgress {
                    UserId = user.Id,
                    StepCurrent = "ProfileCreated",
                    Completed = false,
                    LastUpdated = DateTime.UtcNow
                };
                await _progressRepo.AddOrUpdateAsync(progress);

                var confirmationToken = new ConfirmationToken {
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
                try {
                    await _emailService.SendConfirmationAsync(driverProfile.User.Email, confirmationToken.Token);
                } catch (Exception ex) {
                    throw new OnboardingStepException("Failed to send confirmation email.", "failure_send_confirmationemail", ex);
                }

                return new AuthTokenResponse {
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken,
                    ExpiresIn = token.ExpiresAt,
                    Success = true,
                    UserId = user.Id
                };
            } catch (OnboardingStepException oex) {
                await transaction.RollbackAsync();
                throw;
            } catch (Exception ex) {
                await transaction.RollbackAsync();
                throw new OnboardingStepException("Unexpected onboarding failure.", "unhandled_exception", ex);
            }
        });
    }


    public Task StartAsync(StartOnboardingRequest request, Guid userId) {
        // TODO: Implement verification (e.g., via SMS/email provider)
        throw new NotImplementedException();
    }

    public Task<bool> VerifyCodeAsync(VerifyCodeRequest request, Guid userId) {
        // TODO: Validate code from provider or stored table
        throw new NotImplementedException();
    }

    public async Task<OnboardingProgressResponse> GetProgressAsync(Guid userId) {
        var progress = await _progressRepo.GetByUserIdAsync(userId);
        return progress is null
            ? new OnboardingProgressResponse { StepCurrent = "NotStarted", Completed = false }
            : new OnboardingProgressResponse {
                StepCurrent = progress.StepCurrent,
                Completed = progress.Completed,
                LastUpdated = progress.LastUpdated
            };
    }

    public Task<OnboardingVerificationResult> VerifyIfOperationSuccessfulAsync(string email) {
        // No implementation for this method as the mobile app (TruckeroApp) directly calls
        // the specific API Controller endpoints from its OnboardingApiClientService.
        throw new NotImplementedException();
    }

    public async Task<OperationResult> SendConfirmationEmailAsync(Guid userId) {
        // Find user
        var user = await _userRepo.GetUserByIdAsync(userId);
        if (user == null)
            return OperationResult.Failed("User not found.");

        // Generate new confirmation token
        var confirmationToken = new ConfirmationToken {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Guid.NewGuid().ToString(),
            Used = false,
            CreatedAt = DateTime.UtcNow,
            Type = ConfirmationTokenType.EmailConfirmation
        };
        try {
            await _confirmationTokenRepo.AddConfirmationTokenAsync(confirmationToken);
            await _confirmationTokenRepo.SaveConfirmationTokenChangesAsync();

            await _emailService.SendConfirmationAsync(user.Email, confirmationToken.Token);

            return OperationResult.Succeeded("Confirmation email sent.");
        } catch (Exception ex) {
            return OperationResult.Failed($"Failed to send confirmation email: {ex.Message}");
        }
    }

    public async Task<OperationResult> ConfirmEmailAsync(string token) {
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
}
