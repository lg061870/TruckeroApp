using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Trucks;

namespace Truckero.Core.Interfaces.Services;

public interface ITruckService
{
    Task<List<TruckRequest>> GetDriverTrucksAsync(Guid userId);
    Task<TruckResponse> AddDriverTruckAsync(Guid userId, TruckRequest truck);
    Task<TruckResponse> UpdateDriverTruckAsync(Guid userId, Guid truckId, TruckRequest truck);
    Task<TruckResponse> DeleteDriverTruckAsync(Guid userId, Guid truckId);
    Task<IEnumerable<TruckRequest>> GetTrucksForDriverAsync(Guid driverProfileId);
    Task<List<TruckMake>> GetTruckMakesAsync();
    Task<List<TruckModel>> GetTruckModelsAsync(Guid? makeId = null);
    Task<List<TruckCategory>> GetTruckCategoriesAsync();
    Task<List<BedType>> GetBedTypesAsync();
    Task<List<UseTag>> GetUseTagsAsync();
    Task<List<TruckType>> GetTruckTypesAsync();
    Task<TruckReferenceData> GetTruckPageDataAsync();
}
