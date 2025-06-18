using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

public interface IPaymentMethodRepository
{
    Task<PaymentMethod?> GetPaymentMethodByIdAsync(Guid paymentMethodId);
    Task<List<PaymentMethod>> GetPaymentMethodsByUserIdAsync(Guid userId);
    Task<PaymentMethod?> GetDefaultPaymentMethodByUserIdAsync(Guid userId); // Assuming PaymentMethod can be default
    Task AddPaymentMethodAsync(PaymentMethod paymentMethod);
    Task UpdatePaymentMethodAsync(PaymentMethod paymentMethod);
    Task DeletePaymentMethodAsync(Guid paymentMethodId);
    // Task<List<PaymentMethodType>> GetAvailablePaymentMethodTypesAsync(); // This might be duplicative if already in PayoutAccount or a general service
}