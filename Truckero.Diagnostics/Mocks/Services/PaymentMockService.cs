using Truckero.Core.DTOs.PaymentAccount;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Services;

namespace Truckero.Diagnostics.Mocks.Services;

public class PaymentAccountMockService : IPaymentAccountService
{
    public Task<PaymentAccountResponse> AddPaymentAccountAsync(PaymentAccountRequest request) {
        throw new NotImplementedException();
    }

    public Task AddPaymentMethodAsync(Guid userId, PaymentAccount paymentMethod) {
        throw new NotImplementedException();
    }

    public Task<PaymentAccountResponse> DeletePaymentAccountAsync(Guid userId, Guid paymentAccountId) {
        throw new NotImplementedException();
    }

    public Task DeletePaymentMethodAsync(Guid userId, Guid paymentMethodId) {
        throw new NotImplementedException();
    }

    public Task<List<PaymentMethodType>> GetAllPaymentMethods(string countryCode)
    {
        var dynamicMethods = new List<PaymentMethodType>();

        if (countryCode.ToUpper() == "CRI")
        {
            dynamicMethods.Add(new PaymentMethodType
            {
                Id = Guid.NewGuid(),
                Name = "SINPE",
                Description = "Costa Rica bank transfer via SINPE"
            });
        }

        return Task.FromResult(dynamicMethods);
    }
    public Task<List<PaymentMethodType>> GetAllPayoutMethods(string countryCode)
    {
        var payoutMethods = new List<PaymentMethodType>
    {
        new() { Id = Guid.NewGuid(), Name = "BankAccount", Description = "Deposit directly to bank account" },
        new() { Id = Guid.NewGuid(), Name = "SINPE", Description = "Costa Rica SINPE Móvil" },
        new() { Id = Guid.NewGuid(), Name = "PayPal", Description = "Connect with PayPal" }
    };

        return Task.FromResult(payoutMethods);
    }

    public Task<PaymentAccountResponse> GetPaymentAccountByIdAsync(Guid paymentAccountId) {
        throw new NotImplementedException();
    }

    public Task<PaymentAccountResponse> GetPaymentAccountsByUserIdAsync(Guid userId) {
        throw new NotImplementedException();
    }

    public Task<PaymentAccount?> GetPaymentMethodByIdAsync(Guid paymentMethodId) {
        throw new NotImplementedException();
    }

    public Task<List<PaymentAccount>> GetPaymentMethodsByUserId(Guid userId) {
        throw new NotImplementedException();
    }

    public Task<PaymentAccountResponse> MarkPaymentAccountValidatedAsync(Guid userId, Guid paymentAccountId) {
        throw new NotImplementedException();
    }

    public Task MarkPaymentMethodValidatedAsync(Guid userId, Guid paymentMethodId) {
        throw new NotImplementedException();
    }

    public Task<PaymentAccountResponse> SetDefaultPaymentAccountAsync(Guid userId, Guid paymentAccountId) {
        throw new NotImplementedException();
    }

    public Task SetDefaultPaymentMethodAsync(Guid userId, Guid paymentMethodId) {
        throw new NotImplementedException();
    }

    public Task<PaymentAccountResponse> UpdatePaymentAccountAsync(PaymentAccountRequest request) {
        throw new NotImplementedException();
    }

    public Task UpdatePaymentMethodAsync(Guid userId, Guid paymentMethodId, PaymentAccount updatedMethod) {
        throw new NotImplementedException();
    }
}

