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
            .Include(dp => dp.Vehicles)
            .FirstOrDefaultAsync(dp => dp.UserId == userId);
    }

    public async Task AddAsync(DriverProfile profile)
    {
        await _context.DriverProfiles.AddAsync(profile);
    }

    public Task UpdateAsync(DriverProfile profile)
    {
        _context.DriverProfiles.Update(profile);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<Vehicle>> GetVehiclesAsync(Guid userId)
    {
        return await _context.Vehicles
            .Where(v => v.DriverProfile.UserId == userId)
            .ToListAsync();
    }

    public async Task<Vehicle?> GetVehicleByIdAsync(Guid vehicleId)
    {
        return await _context.Vehicles
            .Include(v => v.VehicleType)
            .FirstOrDefaultAsync(v => v.Id == vehicleId);
    }

    public async Task AddVehicleAsync(Vehicle vehicle)
    {
        await _context.Vehicles.AddAsync(vehicle);
    }

    public Task UpdateVehicleAsync(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
        return Task.CompletedTask;
    }

    public async Task DeleteVehicleAsync(Guid vehicleId)
    {
        var vehicle = await _context.Vehicles.FindAsync(vehicleId);
        if (vehicle != null)
        {
            _context.Vehicles.Remove(vehicle);
        }
    }
}
