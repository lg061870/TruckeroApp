using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<VehicleRepository> _logger;

    public VehicleRepository(AppDbContext context, ILogger<VehicleRepository> logger)
    {
        _context = context;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Vehicle?> GetByIdAsync(Guid id)
    {
        return await _context.Vehicles.FindAsync(id);
    }

    public async Task<IEnumerable<Vehicle>> GetByDriverIdAsync(Guid driverUserId)
    {
        return await _context.Vehicles
            .Where(v => v.DriverProfileId == driverUserId)
            .ToListAsync();
    }

    public async Task AddAsync(Vehicle vehicle)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(vehicle.LicensePlate))
            {
                var exists = await _context.Vehicles
                    .AnyAsync(v =>
                        v.DriverProfileId == vehicle.DriverProfileId &&
                        v.LicensePlate != null &&
                        v.LicensePlate.ToLower() == vehicle.LicensePlate.ToLower());

                if (exists)
                {
                    _logger.LogWarning("Duplicate license plate '{LicensePlate}' for driver {DriverId}", vehicle.LicensePlate, vehicle.DriverProfileId);
                    throw new InvalidOperationException("This license plate already exists for the current driver.");
                }
            }

            // Validate vehicle type exists
            var vehicleTypeExists = await _context.VehicleTypes
                .AnyAsync(vt => vt.Id == vehicle.VehicleTypeId);

            if (!vehicleTypeExists)
            {
                _logger.LogWarning("Invalid VehicleTypeId '{VehicleTypeId}' for driver {DriverId}", vehicle.VehicleTypeId, vehicle.DriverProfileId);
                throw new InvalidOperationException("Invalid VehicleTypeId specified.");
            }

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Vehicle {VehicleId} created for driver {DriverId}", vehicle.Id, vehicle.DriverProfileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while adding vehicle for driver {DriverId}", vehicle.DriverProfileId);
            throw;
        }
    }

    public async Task UpdateAsync(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Vehicle {VehicleId} updated for driver {DriverId}", vehicle.Id, vehicle.DriverProfileId);
    }

    public async Task DeleteAsync(Guid id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle != null)
        {
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Vehicle {VehicleId} deleted for driver {DriverId}", vehicle.Id, vehicle.DriverProfileId);
        }
        else
        {
            _logger.LogWarning("Attempted to delete vehicle {VehicleId} but it was not found.", id);
        }
    }
}
