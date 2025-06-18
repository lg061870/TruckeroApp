using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

public interface ITruckRepository {
    Task<Truck?> GetByIdAsync(Guid id);
    Task<IEnumerable<Truck>> GetByDriverIdAsync(Guid driverUserId);
    Task AddAsync(Truck truck);
    Task UpdateAsync(Truck truck);
    Task DeleteAsync(Guid id);

    Task<IEnumerable<Truck>> GetAllAsync();
    Task<IEnumerable<Truck>> GetAvailableTrucksAsync();

    Task<List<TruckMake>> GetTruckMakesAsync();
    Task<List<TruckModel>> GetTruckModelsAsync(Guid? makeId = null);
    Task<List<TruckCategory>> GetTruckCategoriesAsync();
    Task<List<BedType>> GetBedTypesAsync();
    Task<List<UseTag>> GetUseTagsAsync();
    Task<List<TruckType>> GetTruckTypesAsync();

    /// <summary>
    /// Sets the tags (by ID) associated with a truck.
    /// </summary>
    /// <param name="truckId">The truck's unique identifier.</param>
    /// <param name="tagIds">A list of UseTag IDs to associate with the truck.</param>
    Task AddTagsForTruckAsync(Guid truckId, IEnumerable<Guid> tagIds);
}

