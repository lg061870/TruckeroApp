using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Truckero.Core.Constants;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;

namespace Truckero.Infrastructure.Services {
    public class PayoutAccountService : IPayoutAccountService {
        private readonly IPayoutAccountRepository _payoutAccountRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<PayoutAccountService> _logger;

        public PayoutAccountService(
            IPayoutAccountRepository payoutAccountRepository,
            IUserRepository userRepository,
            ILogger<PayoutAccountService> logger) {
            _payoutAccountRepository = payoutAccountRepository ?? throw new ArgumentNullException(nameof(payoutAccountRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Helper for mapping (customize as needed)
        private static PayoutAccountResponse MapToResponse(PayoutAccount entity, bool success = true, string? message = null, string? errorCode = null) {
            return new PayoutAccountResponse {
                Success = success,
                Message = message,
                PayoutAccount = entity,
                ErrorCode = errorCode
            };
        }

        public async Task<PayoutAccountResponse?> GetPayoutAccountByIdAsync(Guid payoutAccountId) {
            var entity = await _payoutAccountRepository.GetPayoutAccountByIdAsync(payoutAccountId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<List<PayoutAccountResponse>> GetPayoutAccountsByUserIdAsync(Guid userId) {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) {
                _logger.LogWarning("User with ID {UserId} not found when trying to get payout accounts.", userId);
                return new List<PayoutAccountResponse>();
            }
            var entities = await _payoutAccountRepository.GetPayoutAccountsByUserIdAsync(userId);
            return entities.Select(e => MapToResponse(e)).ToList();
        }

        public async Task<PayoutAccountResponse?> GetDefaultPayoutAccountByUserIdAsync(Guid userId) {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) {
                _logger.LogWarning("User with ID {UserId} not found when trying to get default payout account.", userId);
                return null;
            }
            var entity = await _payoutAccountRepository.GetDefaultPayoutAccountByUserIdAsync(userId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<PayoutAccountResponse> AddPayoutAccountAsync(Guid userId, PayoutAccountRequest payoutAccountRequest) {
            var validationContext = new ValidationContext(payoutAccountRequest);
            Validator.ValidateObject(payoutAccountRequest, validationContext, validateAllProperties: true);

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new ReferentialIntegrityException("User not found.", ExceptionCodes.UserNotFound);

            // No direct DBContext access here, assume the repo validates PaymentMethodType existence as needed

            var payoutAccount = new PayoutAccount {
                Id = Guid.NewGuid(),
                UserId = userId,
                PaymentMethodTypeId = payoutAccountRequest.PaymentMethodTypeId,
                AccountNumberLast4 = payoutAccountRequest.AccountNumberLast4,
                IsDefault = payoutAccountRequest.IsDefault,
                MetadataJson = payoutAccountRequest.MetadataJson,
                CreatedAt = DateTime.UtcNow
            };

            await _payoutAccountRepository.AddPayoutAccountAsync(payoutAccount);

            return MapToResponse(payoutAccount);
        }

        public async Task<PayoutAccountResponse> UpdatePayoutAccountAsync(Guid userId, Guid payoutAccountId, PayoutAccountRequest payoutAccountRequest) {
            var validationContext = new ValidationContext(payoutAccountRequest);
            Validator.ValidateObject(payoutAccountRequest, validationContext, validateAllProperties: true);

            var payoutAccount = await _payoutAccountRepository.GetPayoutAccountByIdAsync(payoutAccountId);
            if (payoutAccount == null || payoutAccount.UserId != userId)
                throw new PayoutAccountNotFoundException($"Payout account {payoutAccountId} not found or does not belong to user {userId}.", ExceptionCodes.PayoutAccountNotFound);

            payoutAccount.PaymentMethodTypeId = payoutAccountRequest.PaymentMethodTypeId;
            payoutAccount.AccountNumberLast4 = payoutAccountRequest.AccountNumberLast4;
            payoutAccount.IsDefault = payoutAccountRequest.IsDefault;
            payoutAccount.MetadataJson = payoutAccountRequest.MetadataJson;

            await _payoutAccountRepository.UpdatePayoutAccountAsync(payoutAccount);

            return MapToResponse(payoutAccount);
        }

        public async Task DeletePayoutAccountAsync(Guid userId, Guid payoutAccountId) {
            var payoutAccount = await _payoutAccountRepository.GetPayoutAccountByIdAsync(payoutAccountId);
            if (payoutAccount == null || payoutAccount.UserId != userId)
                throw new PayoutAccountNotFoundException($"Payout account {payoutAccountId} not found or does not belong to user {userId}.", ExceptionCodes.PayoutAccountNotFound);

            if (payoutAccount.IsDefault) {
                var otherAccounts = await _payoutAccountRepository.GetPayoutAccountsByUserIdAsync(userId);
                if (otherAccounts.Any(pa => pa.Id != payoutAccountId))
                    throw new PayoutAccountOperationException("Cannot delete the default payout account when other accounts exist. Set another account as default first.", ExceptionCodes.CannotDeleteDefault);
            }

            await _payoutAccountRepository.DeletePayoutAccountAsync(payoutAccountId);
        }

        public async Task SetDefaultPayoutAccountAsync(Guid userId, Guid payoutAccountId) {
            var accounts = await _payoutAccountRepository.GetPayoutAccountsByUserIdAsync(userId);
            var toSetDefault = accounts.FirstOrDefault(pa => pa.Id == payoutAccountId);
            if (toSetDefault == null)
                throw new PayoutAccountNotFoundException($"Payout account {payoutAccountId} not found for user {userId}.", ExceptionCodes.PayoutAccountNotFound);

            foreach (var acc in accounts)
                acc.IsDefault = acc.Id == payoutAccountId;

            // Only update all accounts if there are changes
            foreach (var acc in accounts)
                await _payoutAccountRepository.UpdatePayoutAccountAsync(acc);
        }

        // These use DbContext in your sample, but for true repository use,
        // you'd add these to a PaymentMethodRepository, BankRepository, CountryRepository, etc.
        // For now, keeping them simple and using placeholder lists.
        public async Task<List<PaymentMethodType>> GetAllPayoutPaymentMethodsAsync() {
            // You need a PaymentMethodRepository in a real scenario.
            throw new NotImplementedException("Implement with repository for PaymentMethodType.");
        }

        public async Task<List<PaymentMethodType>> GetAllPayoutPaymentMethodsByCountryCodeAsync(string countryCode) {
            throw new NotImplementedException("Implement with repository for PaymentMethodType and Bank.");
        }

        public async Task<PayoutPageReferenceDataDto> GetPayoutPageReferenceDataAsync(string countryCode) {
            throw new NotImplementedException("Implement with repositories for PaymentMethodType, Bank, Country.");
        }
    }
}
