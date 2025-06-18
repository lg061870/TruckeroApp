using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Billing; // Assuming a PaymentMethodDto will be here

namespace Truckero.Core.Interfaces.Services;

public interface IPaymentMethodService
{
    Task<PaymentMethod?> GetPaymentMethodByIdAsync(Guid paymentMethodId, Guid userId); // Added userId for ownership check
    Task<List<PaymentMethod>> GetPaymentMethodsByUserIdAsync(Guid userId);
    Task<PaymentMethod?> GetDefaultPaymentMethodByUserIdAsync(Guid userId);
    Task<OperationResult<PaymentMethod>> AddPaymentMethodAsync(Guid userId, PaymentMethodDto paymentMethodDto);
    Task<OperationResult<PaymentMethod>> UpdatePaymentMethodAsync(Guid userId, Guid paymentMethodId, PaymentMethodDto paymentMethodDto);
    Task<OperationResult> DeletePaymentMethodAsync(Guid userId, Guid paymentMethodId);
    Task<OperationResult> SetDefaultPaymentMethodAsync(Guid userId, Guid paymentMethodId);
    Task<List<PaymentMethodType>> GetAvailablePaymentMethodTypesAsync(); // For listing types user can add
}