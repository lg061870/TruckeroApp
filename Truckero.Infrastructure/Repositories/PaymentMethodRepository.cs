using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Truckero.Infrastructure.Repositories;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentMethodRepository> _logger;

    public PaymentMethodRepository(AppDbContext context, ILogger<PaymentMethodRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaymentMethod?> GetPaymentMethodByIdAsync(Guid paymentMethodId)
    {
        try
        {
            return await _context.PaymentMethods
                .Include(pm => pm.PaymentMethodType)
                .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching payment method by ID {PaymentMethodId}", paymentMethodId);
            throw;
        }
    }

    public async Task<List<PaymentMethod>> GetPaymentMethodsByUserIdAsync(Guid userId)
    {
        try
        {
            return await _context.PaymentMethods
                .Where(pm => pm.UserId == userId)
                .Include(pm => pm.PaymentMethodType)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching payment methods for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<PaymentMethod?> GetDefaultPaymentMethodByUserIdAsync(Guid userId)
    {
        try
        {
            return await _context.PaymentMethods
                .Where(pm => pm.UserId == userId && pm.IsDefault)
                .Include(pm => pm.PaymentMethodType)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching default payment method for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task AddPaymentMethodAsync(PaymentMethod paymentMethod)
    {
        if (paymentMethod == null)
            throw new ArgumentNullException(nameof(paymentMethod));

        try
        {
            if (paymentMethod.IsDefault)
            {
                var existingDefaults = await _context.PaymentMethods
                    .Where(pm => pm.UserId == paymentMethod.UserId && pm.IsDefault && pm.Id != paymentMethod.Id)
                    .ToListAsync();
                foreach (var def in existingDefaults)
                {
                    def.IsDefault = false;
                }
            }
            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();
            _logger.LogInformation("PaymentMethod {PaymentMethodId} created for user {UserId}", paymentMethod.Id, paymentMethod.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding payment method for user {UserId}", paymentMethod.UserId);
            throw;
        }
    }

    public async Task UpdatePaymentMethodAsync(PaymentMethod paymentMethod)
    {
        if (paymentMethod == null)
            throw new ArgumentNullException(nameof(paymentMethod));
        
        try
        {
            var existingMethod = await _context.PaymentMethods.FindAsync(paymentMethod.Id);
            if (existingMethod == null)
            {
                _logger.LogWarning("PaymentMethod {PaymentMethodId} not found for update.", paymentMethod.Id);
                throw new KeyNotFoundException($"PaymentMethod with ID {paymentMethod.Id} not found.");
            }

            if (paymentMethod.IsDefault)
            {
                var otherDefaults = await _context.PaymentMethods
                    .Where(pm => pm.UserId == paymentMethod.UserId && pm.IsDefault && pm.Id != paymentMethod.Id)
                    .ToListAsync();
                foreach (var def in otherDefaults)
                {
                    def.IsDefault = false;
                }
            }
            
            _context.Entry(existingMethod).CurrentValues.SetValues(paymentMethod);
            existingMethod.IsDefault = paymentMethod.IsDefault; // Ensure IsDefault is explicitly set

            await _context.SaveChangesAsync();
            _logger.LogInformation("PaymentMethod {PaymentMethodId} updated for user {UserId}", paymentMethod.Id, paymentMethod.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating payment method {PaymentMethodId}", paymentMethod.Id);
            throw;
        }
    }

    public async Task DeletePaymentMethodAsync(Guid paymentMethodId)
    {
        try
        {
            var paymentMethod = await _context.PaymentMethods.FindAsync(paymentMethodId);
            if (paymentMethod != null)
            {
                if (paymentMethod.IsDefault)
                {
                     _logger.LogWarning("Attempted to delete default PaymentMethod {PaymentMethodId}. This is not allowed or requires setting a new default first.", paymentMethodId);
                    // Consider throwing an exception or specific handling
                    return; 
                }
                _context.PaymentMethods.Remove(paymentMethod);
                await _context.SaveChangesAsync();
                _logger.LogInformation("PaymentMethod {PaymentMethodId} deleted", paymentMethodId);
            }
            else
            {
                 _logger.LogWarning("PaymentMethod {PaymentMethodId} not found for deletion.", paymentMethodId);
                throw new KeyNotFoundException($"PaymentMethod with ID {paymentMethodId} not found.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting payment method {PaymentMethodId}", paymentMethodId);
            throw;
        }
    }
}
