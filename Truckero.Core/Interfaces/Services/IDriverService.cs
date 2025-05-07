using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Services;

/// <summary>
/// Interface to be used by client devices that want to call our Driver API Controller
/// </summary>
public interface IDriverService
{
    Task<DriverProfile?> GetByUserIdAsync(Guid userId);
    Task<List<Vehicle>> GetVehiclesAsync(Guid userId);
    Task AddVehicleAsync(Vehicle vehicle);
    Task UpdateVehicleAsync(Vehicle vehicle);
    Task DeleteVehicleAsync(Guid vehicleId);
}
