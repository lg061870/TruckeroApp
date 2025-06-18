using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Onboarding;

namespace Truckero.Core.Interfaces.Services;

public interface IPayoutAccountService
{
    Task<PayoutAccountResponse?> GetPayoutAccountByIdAsync(Guid payoutAccountId);
    Task<List<PayoutAccountResponse>> GetPayoutAccountsByUserIdAsync(Guid userId);
    Task<PayoutAccountResponse?> GetDefaultPayoutAccountByUserIdAsync(Guid userId);
    Task<PayoutAccountResponse> AddPayoutAccountAsync(Guid userId, PayoutAccountRequest payoutAccountRequest);
    Task<PayoutAccountResponse> UpdatePayoutAccountAsync(Guid userId, Guid payoutAccountId, PayoutAccountRequest payoutAccountRequest);
    Task DeletePayoutAccountAsync(Guid userId, Guid payoutAccountId);
    Task SetDefaultPayoutAccountAsync(Guid userId, Guid payoutAccountId);

    Task<List<PaymentMethodType>> GetAllPayoutPaymentMethodsAsync();
    Task<List<PaymentMethodType>> GetAllPayoutPaymentMethodsByCountryCodeAsync(string countryCode);
    Task<PayoutPageReferenceDataDto> GetPayoutPageReferenceDataAsync(string countryCode);
}