using Truckero.Core.Entities;

public interface IVehicleService
{
    Task AddAsync(Vehicle vehicle);
    Task DeleteAsync(Guid vehicleId);
    Task<IEnumerable<Vehicle>> GetVehiclesForDriverAsync(Guid driverProfileId);
    Task UpdateAsync(Vehicle vehicle);
}