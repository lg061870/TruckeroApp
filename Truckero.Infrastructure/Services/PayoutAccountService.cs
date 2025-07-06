using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Truckero.Core.Constants;
using Truckero.Core.DTOs.PayoutAccount;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;

namespace Truckero.Infrastructure.Services;

public class PayoutAccountService : IPayoutAccountService {
    private readonly IPayoutAccountRepository _payoutAccountRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDriverProfileRepository _driverProfileRepository;
    private readonly ILogger<PayoutAccountService> _logger;

    public PayoutAccountService(
        IPayoutAccountRepository payoutAccountRepository,
        IUserRepository userRepository,
        IDriverProfileRepository driverProfileRepository,
        ILogger<PayoutAccountService> logger) {
        _payoutAccountRepository = payoutAccountRepository ?? throw new ArgumentNullException(nameof(payoutAccountRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _driverProfileRepository = driverProfileRepository ?? throw new ArgumentNullException(nameof(driverProfileRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // --- Mapping Helper ---
    private static PayoutAccountRequest MapToDto(PayoutAccount entity) => new() {
        Id = entity.Id,
        UserId = entity.UserId,
        PaymentMethodTypeId = entity.PaymentMethodTypeId,
        AccountNumberLast4 = entity.AccountNumberLast4,
        IsDefault = entity.IsDefault,
        IsValidated = entity.IsValidated,
        MetadataJson = entity.MetadataJson,
        CreatedAt = entity.CreatedAt
    };
    private static PayoutAccountResponse MapToResponse(PayoutAccount entity, bool success = true, string? message = null, string? errorCode = null)
        => new() {
            Success = success,
            Message = message,
            PayoutAccounts = new List<PayoutAccountRequest> { MapToDto(entity) },
            ErrorCode = errorCode
        };
    public async Task<PayoutAccountResponse> GetPayoutAccountByIdAsync(Guid payoutAccountId) {
        var entity = await _payoutAccountRepository.GetPayoutAccountByIdAsync(payoutAccountId);
        return entity == null
            ? new PayoutAccountResponse {
                Success = false,
                Message = "Payout account not found.",
                ErrorCode = ExceptionCodes.PayoutAccountErrorCodes.NotFound
            }
            : MapToResponse(entity);
    }
    public async Task<PayoutAccountResponse> GetPayoutAccountsByUserIdAsync(Guid userId) {
        var driverProfile = await _driverProfileRepository.GetByUserIdAsync(userId);
        if (driverProfile == null) {
            _logger.LogWarning("DriverProfile not found for user {UserId} when querying payout accounts.", userId);
            return new PayoutAccountResponse {
                Success = false,
                Message = "Driver profile is required for payout accounts.",
                ErrorCode = ExceptionCodes.PayoutAccountErrorCodes.DriverProfileNotFound
            };
        }

        var entities = await _payoutAccountRepository.GetPayoutAccountsByUserIdAsync(userId);

        return new PayoutAccountResponse {
            Success = true,
            Message = "Payout accounts retrieved.",
            PayoutAccounts = entities.Select(MapToDto).ToList()
        };
    }
    public async Task<PayoutAccountResponse> GetDefaultPayoutAccountByUserIdAsync(Guid userId) {
        var driverProfile = await _driverProfileRepository.GetByUserIdAsync(userId);
        if (driverProfile == null) {
            _logger.LogWarning("DriverProfile not found for user {UserId} when querying default payout account.", userId);
            throw new PayoutAccountStepException(
                "Driver profile is required for payout accounts.",
                ExceptionCodes.PayoutAccountErrorCodes.DriverProfileNotFound);
        }
        var entity = await _payoutAccountRepository.GetDefaultPayoutAccountByUserIdAsync(userId);
        return entity == null
            ? new PayoutAccountResponse {
                Success = false,
                Message = "Default payout account not found.",
                ErrorCode = ExceptionCodes.PayoutAccountErrorCodes.NotFound
            }
            : MapToResponse(entity);
    }
    public async Task<PayoutAccountResponse> AddPayoutAccountAsync(Guid userId, PayoutAccountRequest request) {
        // 1. Validate DTO
        var validationContext = new ValidationContext(request);
        Validator.ValidateObject(request, validationContext, validateAllProperties: true);

        // 2. User existence
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            throw new PayoutAccountStepException(
                "User not found.",
                ExceptionCodes.PayoutAccountErrorCodes.UserNotFound);

        // 3. Driver profile required!
        var driverProfile = await _driverProfileRepository.GetByUserIdAsync(userId);
        if (driverProfile == null)
            throw new PayoutAccountStepException(
                "Driver profile is required for payout accounts.",
                ExceptionCodes.PayoutAccountErrorCodes.DriverProfileNotFound);

        // 4. Create new entity
        var payoutAccount = new PayoutAccount {
            Id = Guid.NewGuid(),
            UserId = userId,
            PaymentMethodTypeId = request.PaymentMethodTypeId,
            AccountNumberLast4 = request.AccountNumberLast4,
            IsDefault = request.IsDefault,
            IsValidated = request.IsValidated,
            MetadataJson = request.MetadataJson,
            CreatedAt = DateTime.UtcNow
        };

        await _payoutAccountRepository.AddPayoutAccountAsync(payoutAccount);
        await _payoutAccountRepository.SaveChangesAsync();

        return MapToResponse(payoutAccount);
    }
    public async Task<PayoutAccountResponse> UpdatePayoutAccountAsync(Guid userId, Guid payoutAccountId, PayoutAccountRequest request) {
        // 1. Validation
        var validationContext = new ValidationContext(request);
        Validator.ValidateObject(request, validationContext, validateAllProperties: true);

        // 2. Entity existence
        var payoutAccount = await _payoutAccountRepository.GetPayoutAccountByIdAsync(payoutAccountId);
        if (payoutAccount == null || payoutAccount.UserId != userId)
            throw new PayoutAccountNotFoundException(
                $"Payout account {payoutAccountId} not found or does not belong to user {userId}.",
                ExceptionCodes.PayoutAccountErrorCodes.NotFound);

        // 3. (Optional) Driver profile re-check for safety
        var driverProfile = await _driverProfileRepository.GetByUserIdAsync(userId);
        if (driverProfile == null)
            throw new PayoutAccountStepException(
                "Driver profile is required for payout accounts.",
                ExceptionCodes.PayoutAccountErrorCodes.DriverProfileNotFound);

        payoutAccount.PaymentMethodTypeId = request.PaymentMethodTypeId;
        payoutAccount.AccountNumberLast4 = request.AccountNumberLast4;
        payoutAccount.IsDefault = request.IsDefault;
        payoutAccount.IsValidated = request.IsValidated;
        payoutAccount.MetadataJson = request.MetadataJson;

        await _payoutAccountRepository.UpdatePayoutAccountAsync(payoutAccount);
        await _payoutAccountRepository.SaveChangesAsync();

        return MapToResponse(payoutAccount);
    }
    public async Task<PayoutAccountResponse> DeletePayoutAccountAsync(Guid userId, Guid payoutAccountId) {
        var payoutAccount = await _payoutAccountRepository.GetPayoutAccountByIdAsync(payoutAccountId);
        if (payoutAccount == null || payoutAccount.UserId != userId)
            return new PayoutAccountResponse {
                Success = false,
                Message = "Payout account not found or does not belong to user.",
                ErrorCode = ExceptionCodes.PayoutAccountErrorCodes.NotFound
            };

        if (payoutAccount.IsDefault) {
            var otherAccounts = await _payoutAccountRepository.GetPayoutAccountsByUserIdAsync(userId);
            if (otherAccounts.Any(pa => pa.Id != payoutAccountId))
                return new PayoutAccountResponse {
                    Success = false,
                    Message = "Cannot delete the default payout account when other accounts exist. Set another account as default first.",
                    ErrorCode = ExceptionCodes.PayoutAccountErrorCodes.CannotDeleteDefault
                };
        }

        await _payoutAccountRepository.DeletePayoutAccountAsync(payoutAccountId);
        await _payoutAccountRepository.SaveChangesAsync();

        return new PayoutAccountResponse {
            Success = true,
            Message = "Payout account deleted successfully."
        };
    }
    public async Task SetDefaultPayoutAccountAsync(Guid userId, Guid payoutAccountId) {
        var accounts = await _payoutAccountRepository.GetPayoutAccountsByUserIdAsync(userId);
        var toSetDefault = accounts.FirstOrDefault(pa => pa.Id == payoutAccountId);
        if (toSetDefault == null)
            throw new PayoutAccountNotFoundException(
                $"Payout account {payoutAccountId} not found for user {userId}.",
                ExceptionCodes.PayoutAccountErrorCodes.NotFound);

        foreach (var acc in accounts)
            acc.IsDefault = acc.Id == payoutAccountId;

        foreach (var acc in accounts)
            await _payoutAccountRepository.UpdatePayoutAccountAsync(acc);

        await _payoutAccountRepository.SaveChangesAsync();
    }
}
