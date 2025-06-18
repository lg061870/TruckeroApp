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

public class PayoutAccountRepository : IPayoutAccountRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<PayoutAccountRepository> _logger;

    public PayoutAccountRepository(AppDbContext context, ILogger<PayoutAccountRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PayoutAccount?> GetPayoutAccountByIdAsync(Guid payoutAccountId)
    {
        try
        {
            return await _context.PayoutAccounts
                .Include(pa => pa.PaymentMethodType) // Include related PaymentMethodType
                .FirstOrDefaultAsync(pa => pa.Id == payoutAccountId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching payout account by ID {PayoutAccountId}", payoutAccountId);
            throw;
        }
    }

    public async Task<List<PayoutAccount>> GetPayoutAccountsByUserIdAsync(Guid userId)
    {
        try
        {
            return await _context.PayoutAccounts
                .Where(pa => pa.UserId == userId)
                .Include(pa => pa.PaymentMethodType) // Include related PaymentMethodType
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching payout accounts for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<PayoutAccount?> GetDefaultPayoutAccountByUserIdAsync(Guid userId)
    {
        try
        {
            return await _context.PayoutAccounts
                .Where(pa => pa.UserId == userId && pa.IsDefault)
                .Include(pa => pa.PaymentMethodType) // Include related PaymentMethodType
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching default payout account for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task AddPayoutAccountAsync(PayoutAccount payoutAccount)
    {
        if (payoutAccount == null)
            throw new ArgumentNullException(nameof(payoutAccount));

        try
        {
            // If this new account is set as default, ensure no other account for this user is default.
            if (payoutAccount.IsDefault)
            {
                var existingDefaultAccounts = await _context.PayoutAccounts
                    .Where(pa => pa.UserId == payoutAccount.UserId && pa.IsDefault && pa.Id != payoutAccount.Id)
                    .ToListAsync();

                foreach (var acc in existingDefaultAccounts)
                {
                    acc.IsDefault = false;
                    _context.PayoutAccounts.Update(acc);
                }
            }

            _context.PayoutAccounts.Add(payoutAccount);
            await _context.SaveChangesAsync();
            _logger.LogInformation("PayoutAccount {PayoutAccountId} created for user {UserId}", payoutAccount.Id, payoutAccount.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding payout account for user {UserId}", payoutAccount.UserId);
            throw;
        }
    }

    public async Task UpdatePayoutAccountAsync(PayoutAccount payoutAccount)
    {
        if (payoutAccount == null)
            throw new ArgumentNullException(nameof(payoutAccount));

        try
        {
            var existingAccount = await _context.PayoutAccounts.FindAsync(payoutAccount.Id);
            if (existingAccount == null)
            {
                _logger.LogWarning("PayoutAccount {PayoutAccountId} not found for update.", payoutAccount.Id);
                throw new KeyNotFoundException($"PayoutAccount with ID {payoutAccount.Id} not found.");
            }

            // If this account is being set as default, ensure no other account for this user is default.
            if (payoutAccount.IsDefault)
            {
                var otherDefaultAccounts = await _context.PayoutAccounts
                    .Where(pa => pa.UserId == payoutAccount.UserId && pa.IsDefault && pa.Id != payoutAccount.Id)
                    .ToListAsync();

                foreach (var acc in otherDefaultAccounts)
                {
                    acc.IsDefault = false;
                    // _context.PayoutAccounts.Update(acc); // EF Core tracks changes, explicit update might not be needed if already tracked
                }
            }
            
            // Update properties of the existing tracked entity
            _context.Entry(existingAccount).CurrentValues.SetValues(payoutAccount);
            existingAccount.IsDefault = payoutAccount.IsDefault; // Ensure IsDefault is explicitly set

            await _context.SaveChangesAsync();
            _logger.LogInformation("PayoutAccount {PayoutAccountId} updated for user {UserId}", payoutAccount.Id, payoutAccount.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating payout account {PayoutAccountId}", payoutAccount.Id);
            throw;
        }
    }

    public async Task DeletePayoutAccountAsync(Guid payoutAccountId)
    {
        try
        {
            var payoutAccount = await _context.PayoutAccounts.FindAsync(payoutAccountId);
            if (payoutAccount != null)
            {
                if (payoutAccount.IsDefault)
                {
                    _logger.LogWarning("Attempted to delete default PayoutAccount {PayoutAccountId}. This is not allowed or requires setting a new default first.", payoutAccountId);
                    // Optionally, you could throw an InvalidOperationException here or handle it by setting another account as default.
                    // For now, we'll just log and not delete.
                    // throw new InvalidOperationException("Cannot delete the default payout account. Set another account as default first.");
                    return; // Or handle differently
                }
                _context.PayoutAccounts.Remove(payoutAccount);
                await _context.SaveChangesAsync();
                _logger.LogInformation("PayoutAccount {PayoutAccountId} deleted", payoutAccountId);
            }
            else
            {
                _logger.LogWarning("PayoutAccount {PayoutAccountId} not found for deletion.", payoutAccountId);
                throw new KeyNotFoundException($"PayoutAccount with ID {payoutAccountId} not found.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting payout account {PayoutAccountId}", payoutAccountId);
            throw;
        }
    }
}
