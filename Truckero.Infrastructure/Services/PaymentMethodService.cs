using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Billing;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly AppDbContext _dbContext;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PaymentMethodService> _logger;

    public PaymentMethodService(
        AppDbContext dbContext,
        IPaymentMethodRepository paymentMethodRepository,
        IUserRepository userRepository,
        ILogger<PaymentMethodService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _paymentMethodRepository = paymentMethodRepository ?? throw new ArgumentNullException(nameof(paymentMethodRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaymentMethod?> GetPaymentMethodByIdAsync(Guid paymentMethodId, Guid userId)
    {
        var paymentMethod = await _paymentMethodRepository.GetPaymentMethodByIdAsync(paymentMethodId);
        if (paymentMethod != null && paymentMethod.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to access payment method {PaymentMethodId} they do not own.", userId, paymentMethodId);
            return null; // Or throw an access denied exception
        }
        return paymentMethod;
    }

    public async Task<List<PaymentMethod>> GetPaymentMethodsByUserIdAsync(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found when fetching payment methods.", userId);
            return new List<PaymentMethod>();
        }
        return await _paymentMethodRepository.GetPaymentMethodsByUserIdAsync(userId);
    }

    public async Task<PaymentMethod?> GetDefaultPaymentMethodByUserIdAsync(Guid userId)
    {
         var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found when fetching default payment method.", userId);
            return null;
        }
        return await _paymentMethodRepository.GetDefaultPaymentMethodByUserIdAsync(userId);
    }

    public async Task<OperationResult<PaymentMethod>> AddPaymentMethodAsync(Guid userId, PaymentMethodDto paymentMethodDto)
    {
        var validationContext = new ValidationContext(paymentMethodDto);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(paymentMethodDto, validationContext, validationResults, true))
        {
            return OperationResult<PaymentMethod>.Failed(string.Join("; ", validationResults.Select(vr => vr.ErrorMessage)), "VALIDATION_ERROR");
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null) throw new ReferentialIntegrityException("User not found.", "USER_NOT_FOUND");

                var pmType = await _dbContext.PaymentMethodTypes.FindAsync(paymentMethodDto.PaymentMethodTypeId);
                if (pmType == null || !pmType.IsForPayment) throw new ReferentialIntegrityException("Invalid payment method type.", "INVALID_PM_TYPE");

                var paymentMethod = new PaymentMethod
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PaymentMethodTypeId = paymentMethodDto.PaymentMethodTypeId,
                    TokenizedId = paymentMethodDto.TokenizedId,
                    Last4 = paymentMethodDto.Last4,
                    IsDefault = paymentMethodDto.IsDefault,
                    MetadataJson = paymentMethodDto.MetadataJson,
                    CreatedAt = DateTime.UtcNow
                };

                await _paymentMethodRepository.AddPaymentMethodAsync(paymentMethod);
                await transaction.CommitAsync();
                return OperationResult<PaymentMethod>.Succeeded(paymentMethod, "Payment method added.");
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "DB error adding payment method for user {UserId}", userId);
                throw new PaymentMethodStepException("Database error adding payment method.", "DB_ADD_PM_ERROR", dbEx);
            }
            catch (ReferentialIntegrityException ex)
            {
                await transaction.RollbackAsync();
                return OperationResult<PaymentMethod>.Failed(ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unexpected error adding payment method for user {UserId}", userId);
                throw new PaymentMethodStepException("Unexpected error adding payment method.", "UNHANDLED_ADD_PM_ERROR", ex);
            }
        });
    }

    public async Task<OperationResult<PaymentMethod>> UpdatePaymentMethodAsync(Guid userId, Guid paymentMethodId, PaymentMethodDto paymentMethodDto)
    {
         var validationContext = new ValidationContext(paymentMethodDto);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(paymentMethodDto, validationContext, validationResults, true))
        {
            return OperationResult<PaymentMethod>.Failed(string.Join("; ", validationResults.Select(vr => vr.ErrorMessage)), "VALIDATION_ERROR");
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var paymentMethod = await _paymentMethodRepository.GetPaymentMethodByIdAsync(paymentMethodId);
                if (paymentMethod == null || paymentMethod.UserId != userId) throw new PaymentMethodNotFoundException("Payment method not found or access denied.", "PM_NOT_FOUND");

                var pmType = await _dbContext.PaymentMethodTypes.FindAsync(paymentMethodDto.PaymentMethodTypeId);
                if (pmType == null || !pmType.IsForPayment) throw new ReferentialIntegrityException("Invalid payment method type.", "INVALID_PM_TYPE");

                paymentMethod.PaymentMethodTypeId = paymentMethodDto.PaymentMethodTypeId;
                paymentMethod.TokenizedId = paymentMethodDto.TokenizedId; // Be careful with updating tokens
                paymentMethod.Last4 = paymentMethodDto.Last4;
                paymentMethod.IsDefault = paymentMethodDto.IsDefault;
                paymentMethod.MetadataJson = paymentMethodDto.MetadataJson;

                await _paymentMethodRepository.UpdatePaymentMethodAsync(paymentMethod);
                await transaction.CommitAsync();
                return OperationResult<PaymentMethod>.Succeeded(paymentMethod, "Payment method updated.");
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                 _logger.LogError(dbEx, "DB error updating payment method {PaymentMethodId}", paymentMethodId);
                throw new PaymentMethodStepException("Database error updating payment method.", "DB_UPDATE_PM_ERROR", dbEx);
            }
            catch (ReferentialIntegrityException ex)
            {
                await transaction.RollbackAsync();
                return OperationResult<PaymentMethod>.Failed(ex.Message, ex.ErrorCode);
            }
             catch (PaymentMethodNotFoundException ex)
            {
                await transaction.RollbackAsync();
                return OperationResult<PaymentMethod>.Failed(ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unexpected error updating payment method {PaymentMethodId}", paymentMethodId);
                throw new PaymentMethodStepException("Unexpected error updating payment method.", "UNHANDLED_UPDATE_PM_ERROR", ex);
            }
        });
    }

    public async Task<OperationResult> DeletePaymentMethodAsync(Guid userId, Guid paymentMethodId)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var paymentMethod = await _paymentMethodRepository.GetPaymentMethodByIdAsync(paymentMethodId);
                if (paymentMethod == null || paymentMethod.UserId != userId) throw new PaymentMethodNotFoundException("Payment method not found or access denied.", "PM_NOT_FOUND");
                
                if (paymentMethod.IsDefault)
                {
                    var otherMethodsExist = (await _paymentMethodRepository.GetPaymentMethodsByUserIdAsync(userId)).Any(pm => pm.Id != paymentMethodId);
                    if (otherMethodsExist) throw new PaymentMethodOperationException("Cannot delete default payment method if others exist.", "CANNOT_DELETE_DEFAULT_PM");
                }

                await _paymentMethodRepository.DeletePaymentMethodAsync(paymentMethodId);
                await transaction.CommitAsync();
                return OperationResult.Succeeded("Payment method deleted.");
            }
            catch (PaymentMethodNotFoundException ex)
            {
                await transaction.RollbackAsync();
                return OperationResult.Failed(ex.Message, ex.ErrorCode);
            }
            catch (PaymentMethodOperationException ex)
            {
                await transaction.RollbackAsync();
                return OperationResult.Failed(ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unexpected error deleting payment method {PaymentMethodId}", paymentMethodId);
                throw new PaymentMethodStepException("Unexpected error deleting payment method.", "UNHANDLED_DELETE_PM_ERROR", ex);
            }
        });
    }

    public async Task<OperationResult> SetDefaultPaymentMethodAsync(Guid userId, Guid paymentMethodId)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var userMethods = await _paymentMethodRepository.GetPaymentMethodsByUserIdAsync(userId);
                var methodToSetDefault = userMethods.FirstOrDefault(pm => pm.Id == paymentMethodId);

                if (methodToSetDefault == null) throw new PaymentMethodNotFoundException("Payment method not found.", "PM_NOT_FOUND");

                foreach (var pm in userMethods)
                {
                    pm.IsDefault = (pm.Id == paymentMethodId);
                }
                // The repository Update method should handle SaveChanges for each, or do a single SaveChanges here.
                await _dbContext.SaveChangesAsync(); // Assuming repo methods don't save individually in this loop

                await transaction.CommitAsync();
                return OperationResult.Succeeded("Default payment method set.");
            }
            catch (PaymentMethodNotFoundException ex)
            {
                await transaction.RollbackAsync();
                return OperationResult.Failed(ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unexpected error setting default payment method for user {UserId}", userId);
                throw new PaymentMethodStepException("Unexpected error setting default payment method.", "UNHANDLED_SET_DEFAULT_PM_ERROR", ex);
            }
        });
    }

    public async Task<List<PaymentMethodType>> GetAvailablePaymentMethodTypesAsync()
    {
        return await _dbContext.PaymentMethodTypes
            .Where(pmt => pmt.IsForPayment) // Only types usable for making payments
            .AsNoTracking()
            .ToListAsync();
    }
}

// Custom Exceptions (should be in Truckero.Core.Exceptions)
public class PaymentMethodStepException : BaseStepException
{
    public PaymentMethodStepException(string message, string stepCode, Exception? innerException = null)
        : base(message, stepCode, innerException) { }
}

public class PaymentMethodNotFoundException : BaseStepException
{
    public PaymentMethodNotFoundException(string message, string stepCode = "PM_NOT_FOUND", Exception? innerException = null)
        : base(message, stepCode, innerException) { }
}

public class PaymentMethodOperationException : BaseStepException
{
    public PaymentMethodOperationException(string message, string stepCode, Exception? innerException = null)
        : base(message, stepCode, innerException) { }
}