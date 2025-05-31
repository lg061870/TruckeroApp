using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

public interface IDriverRepository
{
    Task<DriverProfile?> GetByUserIdAsync(Guid userId);
    Task<DriverProfile?> GetWithVehiclesAsync(Guid userId);
    Task AddDriverProfileAsync(DriverProfile profile);
    Task UpdaDriverProfileteAsync(DriverProfile profile);
    Task SaveDriverProfileChangesAsync();

    // Vehicle subqueries (as driver owns many)
    Task<List<Vehicle>> GetVehiclesAsync(Guid userId);
    Task<Vehicle?> GetVehicleByIdAsync(Guid vehicleId);
    Task AddVehicleAsync(Vehicle vehicle);
    Task UpdateVehicleAsync(Vehicle vehicle);
    Task DeleteVehicleAsync(Guid vehicleId);
}