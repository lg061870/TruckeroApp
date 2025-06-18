using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories; 
public class TruckRepository : ITruckRepository {
    private readonly AppDbContext _context;
    private readonly ILogger<TruckRepository> _logger;

    public TruckRepository(AppDbContext context, ILogger<TruckRepository> logger) {
        _context = context;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Truck>> GetAllAsync() {
        _logger.LogInformation("Getting all trucks");
        return await _context.Trucks
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Truck?> GetByIdAsync(Guid id)
        => await _context.Trucks.FindAsync(id);

    public async Task<IEnumerable<Truck>> GetByDriverIdAsync(Guid driverUserId)
        => await _context.Trucks
            .Where(v => v.DriverProfileId == driverUserId)
            .ToListAsync();

    public async Task<IEnumerable<Truck>> GetAvailableTrucksAsync() {
        _logger.LogInformation("Getting available trucks");
        return await _context.Trucks
            .Where(t => t.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Truck truck) {
        try {
            if (!string.IsNullOrWhiteSpace(truck.LicensePlate)) {
                var exists = await _context.Trucks
                    .AnyAsync(v =>
                        v.DriverProfileId == truck.DriverProfileId &&
                        v.LicensePlate != null &&
                        v.LicensePlate.ToLower() == truck.LicensePlate.ToLower());

                if (exists) {
                    _logger.LogWarning("Duplicate license plate '{LicensePlate}' for driver {DriverId}", truck.LicensePlate, truck.DriverProfileId);
                    throw new InvalidOperationException("This license plate already exists for the current driver.");
                }
            }

            var vehicleTypeExists = await _context.TruckTypes
                .AnyAsync(vt => vt.Id == truck.TruckTypeId);

            if (!vehicleTypeExists) {
                _logger.LogWarning("Invalid VehicleTypeId '{VehicleTypeId}' for driver {DriverId}", truck.TruckTypeId, truck.DriverProfileId);
                throw new InvalidOperationException("Invalid VehicleTypeId specified.");
            }

            _context.Trucks.Add(truck);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Truck {TruckId} created for driver {DriverId}", truck.Id, truck.DriverProfileId);
        } catch (Exception ex) {
            _logger.LogError(ex, "Exception occurred while adding truck for driver {DriverId}", truck.DriverProfileId);
            throw;
        }
    }

    public async Task UpdateAsync(Truck truck) {
        _context.Trucks.Update(truck);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Truck {TruckId} updated for driver {DriverId}", truck.Id, truck.DriverProfileId);
    }

    public async Task DeleteAsync(Guid id) {
        var truck = await _context.Trucks.FindAsync(id);
        if (truck != null) {
            _context.Trucks.Remove(truck);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Truck {TruckId} deleted", id);
        }
        else {
            throw new KeyNotFoundException("Truck not found.");
        }
    }

    public async Task<List<TruckMake>> GetTruckMakesAsync()
        => await _context.TruckMakes.AsNoTracking().ToListAsync();

    public async Task<List<TruckModel>> GetTruckModelsAsync(Guid? makeId = null) {
        var query = _context.TruckModels.AsNoTracking();
        if (makeId.HasValue)
            query = query.Where(m => m.MakeId == makeId.Value);
        return await query.ToListAsync();
    }

    public async Task<List<TruckCategory>> GetTruckCategoriesAsync()
        => await _context.TruckCategories.AsNoTracking().ToListAsync();

    public async Task<List<BedType>> GetBedTypesAsync()
        => await _context.BedTypes.AsNoTracking().ToListAsync();

    public async Task<List<UseTag>> GetUseTagsAsync()
        => await _context.UseTags.AsNoTracking().ToListAsync();

    public async Task<List<TruckType>> GetTruckTypesAsync()
        => await _context.TruckTypes.AsNoTracking().ToListAsync();

    /// <summary>
    /// Replace all tags for a given truck with the provided tag IDs.
    /// </summary>
    public async Task AddTagsForTruckAsync(Guid truckId, IEnumerable<Guid> tagIds) {
        try {
            // Remove old tags
            var existingTags = _context.TruckUseTags.Where(tut => tut.TruckId == truckId);
            _context.TruckUseTags.RemoveRange(existingTags);

            // Add new tags
            var tagEntities = tagIds.Distinct()
                .Select(tagId => new TruckUseTag {
                    TruckId = truckId,
                    UseTagId = tagId
                }).ToList();

            if (tagEntities.Any())
                await _context.TruckUseTags.AddRangeAsync(tagEntities);

            // Save is deferred to the service/transaction!
            _logger.LogInformation("Tags updated for truck {TruckId}", truckId);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to update tags for truck {TruckId}", truckId);
            throw;
        }
    }
}
