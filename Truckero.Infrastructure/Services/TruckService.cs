using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.DTOs.Trucks;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Services;

public class TruckService : ITruckService {
    private readonly AppDbContext _dbContext; // Inject this!
    private readonly ITruckRepository _truckRepository;
    private readonly IDriverRepository _driverProfileRepository;
    private readonly ILogger<TruckService> _logger;

    public TruckService(
        AppDbContext dbContext,
        ITruckRepository truckRepository,
        IDriverRepository driverProfileRepository,
        ILogger<TruckService> logger
    ) {
        _dbContext = dbContext;
        _truckRepository = truckRepository;
        _driverProfileRepository = driverProfileRepository;
        _logger = logger;
    }

    public async Task<List<Truck>> GetDriverTrucksAsync(Guid userId) {
        // You may want to get DriverProfileId from userId here, or leave as-is if userId == driverProfileId
        return (await _truckRepository.GetByDriverIdAsync(userId)).ToList();
    }

    public async Task<TruckResponseDto> AddDriverTruckAsync(Guid userId, TruckRequestDto truckDto) {
        Validator.ValidateObject(truckDto, new ValidationContext(truckDto), validateAllProperties: true);

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () => {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try {
                // 1. Decide which DriverProfileId to use
                Guid driverProfileId = (truckDto.DriverProfileId != Guid.Empty)
                    ? truckDto.DriverProfileId
                    : Guid.Empty;

                // 2. If not provided, look up using userId
                if (driverProfileId == Guid.Empty) {
                    var driverProfile = await _driverProfileRepository.GetByUserIdAsync(userId);
                    if (driverProfile == null) {
                        throw new ReferentialIntegrityException(
                            "Driver profile does not exist for the specified user.",
                            "MISSING_DRIVER_PROFILE"
                        );
                    }
                    driverProfileId = driverProfile.Id;
                }

                // 3. Now create the Truck using the resolved driverProfileId
                var truck = new Truck {
                    DriverProfileId = driverProfileId,
                    TruckTypeId = truckDto.TruckTypeId,
                    TruckMakeId = truckDto.TruckMakeId,
                    TruckModelId = truckDto.TruckModelId,
                    LicensePlate = truckDto.LicensePlate,
                    Year = truckDto.Year,
                    PhotoFrontUrl = truckDto.PhotoFrontUrl,
                    PhotoBackUrl = truckDto.PhotoBackUrl,
                    PhotoLeftUrl = truckDto.PhotoLeftUrl,
                    PhotoRightUrl = truckDto.PhotoRightUrl,
                    LoadCategory = truckDto.LoadCategory,
                    TruckCategoryId = truckDto.TruckCategoryId,
                    BedTypeId = truckDto.BedTypeId,
                    OwnershipType = truckDto.OwnershipType,
                    InsuranceProvider = truckDto.InsuranceProvider,
                    PolicyNumber = truckDto.PolicyNumber,
                    InsuranceDocumentUrl = truckDto.InsuranceDocumentUrl
                };

                await _truckRepository.AddAsync(truck);

                if (truckDto.UseTagIds != null && truckDto.UseTagIds.Any()) {
                    await _truckRepository.AddTagsForTruckAsync(truck.Id, truckDto.UseTagIds);
                }


                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new TruckResponseDto {
                    Success = true,
                    Message = "Truck added.",
                    Truck = truck
                };
            } catch (DbUpdateException dbEx) {
                await transaction.RollbackAsync();
                var message = dbEx.InnerException?.Message ?? dbEx.Message;

                // Detect specific referential integrity failures based on FK name or message
                if (message.Contains("FK_Trucks_TruckTypes_TruckTypeId")) {
                    throw new ReferentialIntegrityException(
                        "The specified truck type does not exist.",
                        "MISSING_TRUCK_TYPE",
                        dbEx
                    );
                }
                if (message.Contains("FK_Trucks_DriverProfiles_DriverProfileId")) {
                    throw new ReferentialIntegrityException(
                        "The specified driver profile does not exist.",
                        "MISSING_DRIVER_PROFILE",
                        dbEx
                    );
                }
                // Add more FK checks as needed

                // General fallback
                throw new ReferentialIntegrityException(
                    "Referential integrity error occurred while adding the truck. Please check your inputs.",
                    "REFERENTIAL_INTEGRITY_VIOLATION",
                    dbEx
                );
            } catch (ReferentialIntegrityException) {
                await transaction.RollbackAsync();
                throw;
            } catch (Exception ex) {
                await transaction.RollbackAsync();
                throw new TruckStepException(
                    "Unexpected truck onboarding failure.",
                    "UNHANDLED_EXCEPTION",
                    ex
                );
            }
        });
    }

    public async Task<TruckResponseDto> UpdateDriverTruckAsync(Guid userId, Guid truckId, TruckRequestDto truckDto)
    {
        var truck = await _truckRepository.GetByIdAsync(truckId);
        if (truck == null || truck.DriverProfileId != userId)
            return new TruckResponseDto { Success = false, Message = "Truck not found or not owned by user." };
        truck.TruckTypeId = truckDto.TruckTypeId;
        truck.TruckMakeId = truckDto.TruckMakeId;
        truck.TruckModelId = truckDto.TruckModelId;
        truck.LicensePlate = truckDto.LicensePlate;
        truck.Year = truckDto.Year;
        truck.PhotoFrontUrl = truckDto.PhotoFrontUrl;
        truck.PhotoBackUrl = truckDto.PhotoBackUrl;
        truck.PhotoLeftUrl = truckDto.PhotoLeftUrl;
        truck.PhotoRightUrl = truckDto.PhotoRightUrl;
        truck.LoadCategory = truckDto.LoadCategory;
        truck.TruckCategoryId = truckDto.TruckCategoryId;
        truck.BedTypeId = truckDto.BedTypeId;
        truck.OwnershipType = truckDto.OwnershipType;
        truck.InsuranceProvider = truckDto.InsuranceProvider;
        truck.PolicyNumber = truckDto.PolicyNumber;
        truck.InsuranceDocumentUrl = truckDto.InsuranceDocumentUrl;
        // UseTags mapping handled elsewhere (e.g., after save)
        await _truckRepository.UpdateAsync(truck);
        return new TruckResponseDto { Success = true, Message = "Truck updated.", Truck = truck };
    }

    public async Task<TruckResponseDto> DeleteDriverTruckAsync(Guid userId, Guid truckId)
    {
        var truck = await _truckRepository.GetByIdAsync(truckId);
        if (truck == null || truck.DriverProfileId != userId)
            return new TruckResponseDto { Success = false, Message = "Truck not found or not owned by user." };
        await _truckRepository.DeleteAsync(truckId);
        return new TruckResponseDto { Success = true, Message = "Truck deleted." };
    }

    public async Task AddAsync(Truck truck)
    {
        await _truckRepository.AddAsync(truck);
    }

    public async Task DeleteAsync(Guid truckId)
    {
        await _truckRepository.DeleteAsync(truckId);
    }

    public async Task<IEnumerable<Truck>> GetTrucksForDriverAsync(Guid driverProfileId)
    {
        return await _truckRepository.GetByDriverIdAsync(driverProfileId);
    }

    public async Task UpdateAsync(Truck truck)
    {
        await _truckRepository.UpdateAsync(truck);
    }

    public async Task<List<TruckMake>> GetTruckMakesAsync()
        => await _truckRepository.GetTruckMakesAsync();

    public async Task<List<TruckModel>> GetTruckModelsAsync(Guid? makeId = null)
        => await _truckRepository.GetTruckModelsAsync(makeId);

    public async Task<List<TruckCategory>> GetTruckCategoriesAsync()
        => await _truckRepository.GetTruckCategoriesAsync();

    public async Task<List<BedType>> GetBedTypesAsync()
        => await _truckRepository.GetBedTypesAsync();

    public async Task<List<UseTag>> GetUseTagsAsync()
        => await _truckRepository.GetUseTagsAsync();

    public async Task<List<TruckType>> GetTruckTypesAsync()
        => await _truckRepository.GetTruckTypesAsync();

    public Task<TruckPageReferenceDataDto> GetTruckPageDataAsync()
    {
        throw new NotImplementedException("GetTruckPageDataAsync is only implemented in the API client.");
    }
}
