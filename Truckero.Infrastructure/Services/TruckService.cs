using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Truckero.Core.Constants;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.DTOs.Trucks;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Extensions;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Services {
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

        public async Task<List<TruckRequest>> GetDriverTrucksAsync(Guid userId) {
            // You may want to get DriverProfileId from userId here, or leave as-is if userId == driverProfileId
            List<Truck> trucks = (await _truckRepository.GetByDriverIdAsync(userId)).ToList();
            return trucks.Select(truck => truck.ToTruckRequestDto()).ToList();
        }

        public async Task<TruckResponse> AddDriverTruckAsync(Guid userId, TruckRequest truckRequest) {
            Validator.ValidateObject(truckRequest, new ValidationContext(truckRequest), validateAllProperties: true);

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () => {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try {
                    // 1. Decide which DriverProfileId to use
                    Guid driverProfileId = (truckRequest.DriverProfileId != Guid.Empty)
                        ? truckRequest.DriverProfileId
                        : Guid.Empty;

                    // 2. If not provided, look up using userId
                    if (driverProfileId == Guid.Empty) {
                        var driverProfile = await _driverProfileRepository.GetByUserIdAsync(userId);
                        if (driverProfile == null) {
                            throw new ReferentialIntegrityException(
                                "Driver profile does not exist for the specified user.",
                                ExceptionCodes.DriverProfileNotFound // <<-- Use constant!
                            );
                        }
                        driverProfileId = driverProfile.Id;
                    }

                    // 3. Now create the Truck using the resolved driverProfileId
                    var truck = truckRequest.ToTruckEntity();
                    truck.DriverProfileId = driverProfileId;

                    await _truckRepository.AddAsync(truck);

                    // UseTagIds to TruckUseTags mapping
                    if (truckRequest.UseTagIds != null && truckRequest.UseTagIds.Any()) {
                        await _truckRepository.AddTagsForTruckAsync(truck.Id, truckRequest.UseTagIds);
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new TruckResponse {
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
                            ExceptionCodes.TruckTypeNotFound,
                            dbEx
                        );
                    }
                    if (message.Contains("FK_Trucks_DriverProfiles_DriverProfileId")) {
                        throw new ReferentialIntegrityException(
                            "The specified driver profile does not exist.",
                            ExceptionCodes.DriverProfileNotFound,
                            dbEx
                        );
                    }
                    // General fallback
                    throw new ReferentialIntegrityException(
                        "Referential integrity error occurred while adding the truck. Please check your inputs.",
                        ExceptionCodes.ReferentialIntegrityViolation,
                        dbEx
                    );
                } catch (ReferentialIntegrityException) {
                    await transaction.RollbackAsync();
                    throw;
                } catch (Exception ex) {
                    await transaction.RollbackAsync();
                    throw new TruckStepException(
                        "Unexpected truck onboarding failure.",
                        ExceptionCodes.UnhandledException,
                        ex
                    );
                }
            });
        }

        public async Task<TruckResponse> UpdateDriverTruckAsync(Guid userId, Guid truckId, TruckRequest truckDto) {
            var truck = await _truckRepository.GetByIdAsync(truckId);
            if (truck == null || truck.DriverProfileId != userId)
                return new TruckResponse { Success = false, Message = "Truck not found or not owned by user." };

            // Map updated values from DTO to entity
            var updatedTruck = truckDto.ToTruckEntity();
            // Preserve original Id and DriverProfileId
            updatedTruck.Id = truck.Id;
            updatedTruck.DriverProfileId = truck.DriverProfileId;

            await _truckRepository.UpdateAsync(updatedTruck);

            // (Optional: Handle UseTagIds update here if you support updating tags)
            // e.g., await _truckRepository.UpdateTagsForTruckAsync(truck.Id, truckDto.UseTagIds);

            return new TruckResponse { Success = true, Message = "Truck updated.", Truck = updatedTruck };
        }

        public async Task<TruckResponse> DeleteDriverTruckAsync(Guid userId, Guid truckId) {
            var truck = await _truckRepository.GetByIdAsync(truckId);
            if (truck == null || truck.DriverProfileId != userId)
                return new TruckResponse { Success = false, Message = "Truck not found or not owned by user." };

            await _truckRepository.DeleteAsync(truckId);
            return new TruckResponse { Success = true, Message = "Truck deleted." };
        }

        public async Task AddAsync(Truck truck) {
            await _truckRepository.AddAsync(truck);
        }

        public async Task DeleteAsync(Guid truckId) {
            await _truckRepository.DeleteAsync(truckId);
        }

        public async Task<IEnumerable<TruckRequest>> GetTrucksForDriverAsync(Guid driverProfileId) {
            var trucks = await _truckRepository.GetByDriverIdAsync(driverProfileId);
            return trucks.Select(truck => truck.ToTruckRequestDto()).ToList();
        }

        public async Task UpdateAsync(Truck truck) {
            await _truckRepository.UpdateAsync(truck);
        }

        // Reference Data Methods (Proxy to repository)
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

        public Task<TruckReferenceData> GetTruckPageDataAsync() {
            throw new NotImplementedException("GetTruckPageDataAsync is only implemented in the API client.");
        }
    }
}
