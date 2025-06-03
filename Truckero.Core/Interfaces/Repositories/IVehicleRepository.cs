using System;
using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

public interface IVehicleRepository
{
    Task<Truck?> GetByIdAsync(Guid id);
    Task<IEnumerable<Truck>> GetByDriverIdAsync(Guid driverUserId);
    Task AddAsync(Truck vehicle);
    Task UpdateAsync(Truck vehicle);
    Task DeleteAsync(Guid id);
}




