using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

public interface IDriverProfileRepository
{
    Task<DriverProfile?> GetByUserIdAsync(Guid userId);
    Task<DriverProfile?> GetWithVehiclesAsync(Guid userId);
    Task AddDriverProfileAsync(DriverProfile profile);
    Task UpdaDriverProfileteAsync(DriverProfile profile);
    Task SaveDriverProfileChangesAsync();

    // Vehicle subqueries (as driver owns many)
    Task<List<Truck>> GetVehiclesAsync(Guid userId);
    Task<Truck?> GetVehicleByIdAsync(Guid vehicleId);
    Task AddVehicleAsync(Truck vehicle);
    Task UpdateVehicleAsync(Truck vehicle);
    Task DeleteVehicleAsync(Guid vehicleId);
}