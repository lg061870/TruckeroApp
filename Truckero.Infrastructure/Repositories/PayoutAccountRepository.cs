using Microsoft.EntityFrameworkCore;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories;

public class PayoutAccountRepository : IPayoutAccountRepository
{
    private readonly AppDbContext _db;

    public PayoutAccountRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PayoutAccount>> GetByUserIdAsync(Guid userId)
    {
        return await _db.PayoutAccounts
            .Where(pa => pa.UserId == userId)
            .Include(pa => pa.PaymentMethodType)
            .ToListAsync();
    }

    public async Task<PayoutAccount?> GetDefaultByUserIdAsync(Guid userId)
    {
        return await _db.PayoutAccounts
            .Where(pa => pa.UserId == userId && pa.IsDefault)
            .Include(pa => pa.PaymentMethodType)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(PayoutAccount account)
    {
        _db.PayoutAccounts.Add(account);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var account = await _db.PayoutAccounts.FindAsync(id);
        if (account != null)
        {
            _db.PayoutAccounts.Remove(account);
            await _db.SaveChangesAsync();
        }
    }
}
