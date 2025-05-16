using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly AppDbContext _context;
    private readonly IAuditLogRepository _audit;

    public VehicleRepository(AppDbContext context, IAuditLogRepository audit)
    {
        _context = context;
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
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
                    await _audit.LogAsync(new AuditLog
                    {
                        UserId = vehicle.DriverProfileId,
                        Action = "VehicleAddFailed",
                        TargetType = "Vehicle",
                        TargetId = Guid.NewGuid(), // not created
                        Timestamp = DateTime.UtcNow,
                        Description = "Duplicate license plate"
                    });

                    throw new InvalidOperationException("This license plate already exists for the current driver.");
                }
            }

            // Validate vehicle type exists
            var vehicleTypeExists = await _context.VehicleTypes
                .AnyAsync(vt => vt.Id == vehicle.VehicleTypeId);

            if (!vehicleTypeExists)
            {
                await _audit.LogAsync(new AuditLog
                {
                    UserId = vehicle.DriverProfileId,
                    Action = "VehicleAddFailed",
                    TargetType = "Vehicle",
                    TargetId = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    Description = "Invalid VehicleTypeId"
                });

                throw new InvalidOperationException("Invalid VehicleTypeId specified.");
            }

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(new AuditLog
            {
                UserId = vehicle.DriverProfileId,
                Action = "VehicleCreated",
                TargetType = "Vehicle",
                TargetId = vehicle.Id,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            // Optionally log unexpected errors (infra, db, etc.)
            await _audit.LogAsync(new AuditLog
            {
                UserId = vehicle.DriverProfileId,
                Action = "VehicleAddException",
                TargetType = "Vehicle",
                TargetId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Description = ex.Message
            });

            throw;
        }
    }

    public async Task UpdateAsync(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle != null)
        {
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
        }
    }
}
