using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories;

public class DriverRepository : IDriverRepository
{
    private readonly AppDbContext _context;

    public DriverRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DriverProfile?> GetByUserIdAsync(Guid userId)
    {
        return await _context.DriverProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(dp => dp.UserId == userId);
    }

    public async Task<DriverProfile?> GetWithVehiclesAsync(Guid userId)
    {
        return await _context.DriverProfiles
            .Include(dp => dp.Trucks)
            .FirstOrDefaultAsync(dp => dp.UserId == userId);
    }

    public async Task AddDriverProfileAsync(DriverProfile profile)
    {
        await _context.DriverProfiles.AddAsync(profile);
    }

    public Task UpdaDriverProfileteAsync(DriverProfile profile)
    {
        _context.DriverProfiles.Update(profile);
        return Task.CompletedTask;
    }

    public async Task SaveDriverProfileChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<Truck>> GetVehiclesAsync(Guid userId)
    {
        return await _context.Trucks
            .Where(v => v.DriverProfile.UserId == userId)
            .ToListAsync();
    }

    public async Task<Truck?> GetVehicleByIdAsync(Guid vehicleId)
    {
        return await _context.Trucks
            .Include(v => v.TruckType)
            .FirstOrDefaultAsync(v => v.Id == vehicleId);
    }

    public async Task AddVehicleAsync(Truck vehicle)
    {
        await _context.Trucks.AddAsync(vehicle);
    }

    public Task UpdateVehicleAsync(Truck vehicle)
    {
        _context.Trucks.Update(vehicle);
        return Task.CompletedTask;
    }

    public async Task DeleteVehicleAsync(Guid vehicleId)
    {
        var vehicle = await _context.Trucks.FindAsync(vehicleId);
        if (vehicle != null)
        {
            _context.Trucks.Remove(vehicle);
        }
    }
}
