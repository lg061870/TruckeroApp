using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories; 
/// <summary>
/// EF Core implementation for CRUD on PayoutAccount.
/// </summary>
public class PayoutAccountRepository : IPayoutAccountRepository {
    private readonly AppDbContext _context;

    public PayoutAccountRepository(AppDbContext context) {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<PayoutAccount?> GetPayoutAccountByIdAsync(Guid payoutAccountId) {
        return await _context.PayoutAccounts
            .Include(pa => pa.PaymentMethodType) // Navigation property if needed
            .FirstOrDefaultAsync(pa => pa.Id == payoutAccountId);
    }

    /// <inheritdoc />
    public async Task<List<PayoutAccount>> GetPayoutAccountsByUserIdAsync(Guid userId) {
        return await _context.PayoutAccounts
            .Include(pa => pa.PaymentMethodType)
            .Where(pa => pa.UserId == userId)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<PayoutAccount?> GetDefaultPayoutAccountByUserIdAsync(Guid userId) {
        return await _context.PayoutAccounts
            .Include(pa => pa.PaymentMethodType)
            .FirstOrDefaultAsync(pa => pa.UserId == userId && pa.IsDefault);
    }

    /// <inheritdoc />
    public async Task AddPayoutAccountAsync(PayoutAccount payoutAccount) {
        await _context.PayoutAccounts.AddAsync(payoutAccount);
        // No SaveChanges here; handled by Unit of Work or service.
    }

    /// <inheritdoc />
    public async Task UpdatePayoutAccountAsync(PayoutAccount payoutAccount) {
        _context.PayoutAccounts.Update(payoutAccount);
        await Task.CompletedTask; // Satisfy async signature, no-op (EF tracks)
    }

    /// <inheritdoc />
    public async Task DeletePayoutAccountAsync(Guid payoutAccountId) {
        var entity = await _context.PayoutAccounts.FindAsync(payoutAccountId);
        if (entity != null) {
            _context.PayoutAccounts.Remove(entity);
        }
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync() {
        await _context.SaveChangesAsync();
    }

    // --- Legacy alias for onboarding, delegates to AddPayoutAccountAsync ---
    public async Task AddAsync(PayoutAccount payoutAccount) =>
        await AddPayoutAccountAsync(payoutAccount);
}
