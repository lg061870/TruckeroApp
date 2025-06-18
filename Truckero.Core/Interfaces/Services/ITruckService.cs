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
    Task<List<Truck>> GetDriverTrucksAsync(Guid userId);
    Task<TruckResponseDto> AddDriverTruckAsync(Guid userId, TruckRequestDto truck);
    Task<TruckResponseDto> UpdateDriverTruckAsync(Guid userId, Guid truckId, TruckRequestDto truck);
    Task<TruckResponseDto> DeleteDriverTruckAsync(Guid userId, Guid truckId);
    Task<IEnumerable<Truck>> GetTrucksForDriverAsync(Guid driverProfileId);
    Task<List<TruckMake>> GetTruckMakesAsync();
    Task<List<TruckModel>> GetTruckModelsAsync(Guid? makeId = null);
    Task<List<TruckCategory>> GetTruckCategoriesAsync();
    Task<List<BedType>> GetBedTypesAsync();
    Task<List<UseTag>> GetUseTagsAsync();
    Task<List<TruckType>> GetTruckTypesAsync();
    Task<TruckPageReferenceDataDto> GetTruckPageDataAsync();
}
