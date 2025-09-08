// File: Truckero.Infrastructure.Services/DriverBidService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Truckero.Core.Constants;
using Truckero.Core.DTOs.CustomerFlow;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Services;

public class DriverBidService : IDriverBidService {
    private readonly AppDbContext _dbContext;
    private readonly IDriverBidRepository _driverBidRepository;
    private readonly ILogger<DriverBidService> _logger;

    public DriverBidService(
        AppDbContext dbContext,
        IDriverBidRepository driverBidRepository,
        ILogger<DriverBidService> logger) {
        _dbContext = dbContext;
        _driverBidRepository = driverBidRepository;
        _logger = logger;
    }

    public async Task<DriverBidResponse> CreateDriverBidAsync(DriverBidRequest request) {
        Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () => {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try {
                // Foreign Key validation
                if (!await _dbContext.FreightBids.AnyAsync(fb => fb.Id == request.FreightBidId))
                    throw new ReferentialIntegrityException(
                        "FreightBid does not exist.",
                        ExceptionCodes.DriverBidErrorCodes.FreightBidNotFound);

                if (!await _dbContext.Users.AnyAsync(u => u.Id == request.DriverId))
                    throw new ReferentialIntegrityException(
                        "Driver does not exist.",
                        ExceptionCodes.DriverBidErrorCodes.DriverNotFound);

                if (!await _dbContext.Trucks.AnyAsync(t => t.Id == request.TruckId))
                    throw new ReferentialIntegrityException(
                        "Truck does not exist.",
                        ExceptionCodes.DriverBidErrorCodes.TruckNotFound);

                var driverBid = ToDriverBidEntity(request);
                await _driverBidRepository.AddDriverBidAsync(driverBid);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new DriverBidResponse {
                    Success = true,
                    Message = "Driver bid created.",
                    DriverBids = new List<DriverBidRequest> { request }
                };
            } catch (DbUpdateException dbEx) {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "DbUpdateException occurred when creating DriverBid.");
                return new DriverBidResponse {
                    Success = false,
                    Message = "Database update error during driver bid creation.",
                    ErrorCode = ExceptionCodes.DriverBidErrorCodes.SaveFailed
                };
            } catch (ReferentialIntegrityException ex) {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "DriverBid referential integrity failed.");
                return new DriverBidResponse {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = ex.ErrorCode
                };
            } catch (Exception ex) {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unhandled exception during driver bid creation.");
                return new DriverBidResponse {
                    Success = false,
                    Message = "Unexpected error occurred while creating driver bid.",
                    ErrorCode = ExceptionCodes.DriverBidErrorCodes.Unknown
                };
            }
        });
    }
    public async Task<DriverBidResponse> UpdateDriverBidAsync(Guid id, DriverBidRequest request) {
        Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () => {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try {
                var existing = await _driverBidRepository.GetDriverBidByIdAsync(id);

                if (!await _dbContext.FreightBids.AnyAsync(fb => fb.Id == request.FreightBidId))
                    throw new ReferentialIntegrityException(
                        "FreightBid does not exist.",
                        ExceptionCodes.DriverBidErrorCodes.FreightBidNotFound);

                if (!await _dbContext.Users.AnyAsync(u => u.Id == request.DriverId))
                    throw new ReferentialIntegrityException(
                        "Driver does not exist.",
                        ExceptionCodes.DriverBidErrorCodes.DriverNotFound);

                if (!await _dbContext.Trucks.AnyAsync(t => t.Id == request.TruckId))
                    throw new ReferentialIntegrityException(
                        "Truck does not exist.",
                        ExceptionCodes.DriverBidErrorCodes.TruckNotFound);

                var entity = ToDriverBidEntity(request);
                entity.Id = id;
                await _driverBidRepository.UpdateDriverBidAsync(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new DriverBidResponse {
                    Success = true,
                    Message = "Driver bid updated.",
                    DriverBids = new List<DriverBidRequest> { request }
                };
            } catch (DriverBidException ex) {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, ex.Message);
                return new DriverBidResponse {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = ExceptionCodes.DriverBidErrorCodes.SaveFailed
                };
            } catch (DbUpdateException dbEx) {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "DbUpdateException occurred when updating DriverBid.");
                return new DriverBidResponse {
                    Success = false,
                    Message = "Database update error during driver bid update.",
                    ErrorCode = ExceptionCodes.DriverBidErrorCodes.SaveFailed
                };
            } catch (Exception ex) {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unhandled exception during driver bid update.");
                return new DriverBidResponse {
                    Success = false,
                    Message = "Unexpected error occurred while updating driver bid.",
                    ErrorCode = ExceptionCodes.DriverBidErrorCodes.Unknown
                };
            }
        });
    }
    public async Task<DriverBidResponse> GetDriverBidByIdAsync(Guid id) {
        try {
            var entity = await _driverBidRepository.GetDriverBidByIdAsync(id);
            if (entity == null) {
                return new DriverBidResponse {
                    Success = false,
                    Message = "Driver bid not found.",
                    ErrorCode = ExceptionCodes.DriverBidErrorCodes.NotFound,
                    DriverBids = new List<DriverBidRequest>()
                };
            }

            return new DriverBidResponse {
                Success = true,
                DriverBids = new List<DriverBidRequest> { ToDriverBidRequest(entity) }
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting driver bid by id.");
            return new DriverBidResponse {
                Success = false,
                Message = ex.Message,
                ErrorCode = ExceptionCodes.DriverBidErrorCodes.Unknown,
                DriverBids = new List<DriverBidRequest>()
            };
        }
    }
    public async Task<DriverBidResponse> GetDriverBidsByFreightBidIdAsync(Guid freightBidId) {
        try {
            var entities = await _driverBidRepository.GetDriverBidsByFreightBidIdAsync(freightBidId);
            return new DriverBidResponse {
                Success = true,
                DriverBids = entities.Select(ToDriverBidRequest).ToList()
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting driver bids by freight bid id.");
            return new DriverBidResponse {
                Success = false,
                Message = ex.Message,
                ErrorCode = ExceptionCodes.DriverBidErrorCodes.Unknown,
                DriverBids = new List<DriverBidRequest>()
            };
        }
    }
    public async Task<DriverBidResponse> GetDriverBidsByDriverIdAsync(Guid driverId) {
        try {
            var entities = await _driverBidRepository.GetDriverBidsByDriverIdAsync(driverId);
            return new DriverBidResponse {
                Success = true,
                DriverBids = entities.Select(ToDriverBidRequest).ToList()
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting driver bids by driver id.");
            return new DriverBidResponse {
                Success = false,
                Message = ex.Message,
                ErrorCode = ExceptionCodes.DriverBidErrorCodes.Unknown,
                DriverBids = new List<DriverBidRequest>()
            };
        }
    }
    public async Task<DriverBidResponse> DeleteDriverBidAsync(Guid id) {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () => {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try {
                await _driverBidRepository.DeleteDriverBidAsync(id);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new DriverBidResponse {
                    Success = true,
                    Message = "Driver bid deleted.",
                    DriverBids = new List<DriverBidRequest>()
                };
            } catch (DbUpdateException dbEx) {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "DbUpdateException during DriverBid delete.");
                return new DriverBidResponse {
                    Success = false,
                    Message = "Database update error during DriverBid deletion.",
                    ErrorCode = ExceptionCodes.DriverBidErrorCodes.SaveFailed,
                    DriverBids = new List<DriverBidRequest>()
                };
            } catch (Exception ex) {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unhandled exception during DriverBid delete.");
                return new DriverBidResponse {
                    Success = false,
                    Message = "Unexpected error occurred while deleting DriverBid.",
                    ErrorCode = ExceptionCodes.DriverBidErrorCodes.Unknown,
                    DriverBids = new List<DriverBidRequest>()
                };
            }
        });
    }
    
    public async Task<FindDriversStatusResponse> GetFindDriversStatusAsync(Guid freightBidId) {
        try {
            // Verify the freight bid exists
            var freightBid = await _dbContext.FreightBids
                .FirstOrDefaultAsync(fb => fb.Id == freightBidId);
                
            if (freightBid == null) {
                return new FindDriversStatusResponse {
                    Success = false,
                    Message = "Freight bid not found.",
                    ErrorCode = ExceptionCodes.DriverBidErrorCodes.FreightBidNotFound,
                    BidsStatuses = new List<FindDriversStatusRequest>(),
                    NotifiedDrivers = 0
                };
            }
            
            // Get driver bids for the freight bid
            var driverBids = await _driverBidRepository.GetDriverBidsByFreightBidIdAsync(freightBidId);
            
            // Create the status request
            var status = new FindDriversStatusRequest(
                FreightBidId: freightBidId,
                DriversFound: driverBids.Any(),
                TotalDriversFound: driverBids.Count,
                RequestTime: DateTime.UtcNow,
                StatusMessage: driverBids.Any()
                    ? $"Found {driverBids.Count} driver(s) for this freight bid"
                    : "No drivers found for this freight bid"
            );
            
            return new FindDriversStatusResponse {
                Success = true,
                Message = "Driver search status retrieved successfully.",
                BidsStatuses = new List<FindDriversStatusRequest> { status },
                NotifiedDrivers = driverBids.Count // Add the NotifiedDrivers property value
            };
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error retrieving driver search status for freight bid {FreightBidId}", freightBidId);
            return new FindDriversStatusResponse {
                Success = false,
                Message = "An error occurred while retrieving driver search status.",
                ErrorCode = ExceptionCodes.DriverBidErrorCodes.Unknown,
                BidsStatuses = new List<FindDriversStatusRequest>(),
                NotifiedDrivers = 0
            };
        }
    }

    // ---------- Mapping Helpers ----------
    private static DriverBid ToDriverBidEntity(DriverBidRequest request) {
        return new DriverBid {
            FreightBidId = request.FreightBidId,
            DriverProfileId = request.DriverId,
            TruckId = request.TruckId,
            OfferAmount = request.OfferAmount,
            Message = request.Message,
            // Set SubmittedAt and Status as needed
        };
    }
    private static DriverBidRequest ToDriverBidRequest(DriverBid entity) {
        return new DriverBidRequest {
            FreightBidId = entity.FreightBidId,
            DriverId = entity.DriverProfileId,
            TruckId = entity.TruckId,
            OfferAmount = entity.OfferAmount,
            Message = entity.Message
        };
    }
}
