using Truckero.Core.Entities;

public interface IVehicleService
{
    Task AddAsync(Truck vehicle);
    Task DeleteAsync(Guid vehicleId);
    Task<IEnumerable<Truck>> GetVehiclesForDriverAsync(Guid driverProfileId);
    Task UpdateAsync(Truck vehicle);
}