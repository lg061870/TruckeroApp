using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories; 
public class PaymentAccountRepository : IPaymentAccountRepository {
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentAccountRepository> _logger;

    public PaymentAccountRepository(AppDbContext context, ILogger<PaymentAccountRepository> logger) {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // --- Query Methods ---

    public async Task<List<PaymentAccount>> GetPaymentAccountsByUserIdAsync(Guid userId) {
        return await _context.PaymentAccounts
            .Where(pa => pa.UserId == userId)
            .Include(pa => pa.PaymentMethodType)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<PaymentAccount?> GetPaymentAccountByIdAsync(Guid paymentAccountId) {
        return await _context.PaymentAccounts
            .Include(pa => pa.PaymentMethodType)
            .AsNoTracking()
            .FirstOrDefaultAsync(pa => pa.Id == paymentAccountId);
    }

    public async Task<PaymentAccount?> GetDefaultPaymentAccountByUserIdAsync(Guid userId) {
        return await _context.PaymentAccounts
            .Where(pa => pa.UserId == userId && pa.IsDefault)
            .Include(pa => pa.PaymentMethodType)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    // --- Command Methods (Do NOT call SaveChanges inside these) ---

    public async Task AddPaymentAccountAsync(PaymentAccount paymentAccount) {
        if (paymentAccount == null)
            throw new ArgumentNullException(nameof(paymentAccount));

        _context.PaymentAccounts.Add(paymentAccount);
    }

    public async Task UpdatePaymentAccountAsync(PaymentAccount paymentAccount) {
        if (paymentAccount == null)
            throw new ArgumentNullException(nameof(paymentAccount));

        _context.PaymentAccounts.Update(paymentAccount);
    }

    public async Task DeletePaymentAccountAsync(Guid paymentAccountId) {
        var account = await _context.PaymentAccounts.FindAsync(paymentAccountId);
        if (account != null) {
            _context.PaymentAccounts.Remove(account);
        }
    }

    public async Task SetDefaultPaymentAccountAsync(Guid userId, Guid paymentAccountId) {
        var userAccounts = await _context.PaymentAccounts
            .Where(pa => pa.UserId == userId)
            .ToListAsync();

        foreach (var acc in userAccounts) {
            acc.IsDefault = (acc.Id == paymentAccountId);
        }
    }

    public async Task MarkPaymentAccountValidatedAsync(Guid userId, Guid paymentAccountId) {
        var account = await _context.PaymentAccounts
            .FirstOrDefaultAsync(pa => pa.UserId == userId && pa.Id == paymentAccountId);

        if (account != null) {
            account.IsValidated = true;
        }
    }

    // --- Save Changes ---
    public async Task SaveChangesAsync() {
        await _context.SaveChangesAsync();
    }
}
