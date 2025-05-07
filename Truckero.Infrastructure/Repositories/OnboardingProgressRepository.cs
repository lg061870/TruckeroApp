using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories;

public class OnboardingProgressRepository : IOnboardingProgressRepository
{
    private readonly AppDbContext _context;

    public OnboardingProgressRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OnboardingProgress?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Onboardings
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task AddOrUpdateAsync(OnboardingProgress progress)
    {
        var existing = await GetByUserIdAsync(progress.UserId);
        if (existing is null)
        {
            await _context.Onboardings.AddAsync(progress);
        }
        else
        {
            existing.StepCurrent = progress.StepCurrent;
            existing.Completed = progress.Completed;
            existing.LastUpdated = DateTime.UtcNow;
            _context.Onboardings.Update(existing);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

