using System;
using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

// This interface has been replaced by ITruckRepository. Use ITruckRepository instead.
// (File retained for migration reference)
public interface IVehicleRepository
{
    Task<Truck?> GetByIdAsync(Guid id);
    Task<IEnumerable<Truck>> GetByDriverIdAsync(Guid driverUserId);
    Task AddAsync(Truck vehicle);
    Task UpdateAsync(Truck vehicle);
    Task DeleteAsync(Guid id);
}




