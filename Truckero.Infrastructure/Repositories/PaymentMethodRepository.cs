using Microsoft.EntityFrameworkCore;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly AppDbContext _db;

    public PaymentMethodRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PaymentMethod>> GetByUserIdAsync(Guid userId)
    {
        return await _db.PaymentMethods
            .Where(pm => pm.UserId == userId)
            .Include(pm => pm.PaymentMethodType)
            .ToListAsync();
    }

    public async Task<PaymentMethod?> GetDefaultByUserIdAsync(Guid userId)
    {
        return await _db.PaymentMethods
            .Where(pm => pm.UserId == userId && pm.IsDefault)
            .Include(pm => pm.PaymentMethodType)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(PaymentMethod method)
    {
        _db.PaymentMethods.Add(method);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var method = await _db.PaymentMethods.FindAsync(id);
        if (method != null)
        {
            _db.PaymentMethods.Remove(method);
            await _db.SaveChangesAsync();
        }
    }
}
