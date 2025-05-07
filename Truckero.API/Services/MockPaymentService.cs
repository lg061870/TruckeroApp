namespace Truckero.API.Services;

using Truckero.Core.Interfaces.Services;

public class MockPaymentService : IPaymentService
{
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

}

