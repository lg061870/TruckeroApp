using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Truckero.Core.Constants;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.CustomerFlow;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Services;
using Truckero.Infrastructure.Data;
using Truckero.Infrastructure.Repositories;
using static Truckero.Core.Constants.ExceptionCodes;

namespace Truckero.Infrastructure.Services;

public class FreightBidService : IFreightBidService {
    private readonly AppDbContext _dbContext;
    private readonly IFreightBidRepository _freightBidRepository;
    private readonly IDriverBidRepository _driverBidRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICustomerProfileRepository _customerProfileRepository;
    private readonly ILogger<FreightBidService> _logger;

    public FreightBidService(
        AppDbContext dbContext,
        IFreightBidRepository freightBidRepository,
        IDriverBidRepository driverBidRepository,
        IUserRepository userRepository, 
        ICustomerProfileRepository customerProfileRepository,
        ILogger<FreightBidService> logger) {
        _dbContext = dbContext;
        _freightBidRepository = freightBidRepository;
        _driverBidRepository = driverBidRepository;
        _userRepository = userRepository;
        _customerProfileRepository = customerProfileRepository;
        _logger = logger;
    }

    // ---- FreightBid ----

    private FreightBidResponse ErrorResponse(string message, string code) => new FreightBidResponse {
        Success = false,
        ErrorCode = code,
        Message = message,
        FreightBids = new List<FreightBidRequest>() // Typically empty on error
    };

    public async Task<FreightBidResponse> CreateFreightBidAsync(FreightBidRequest request) {
        try {
            Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try {
                    // Step 1: Add FreightBid (but do not assign navigation collections yet)
                    var entity = ToFreightBidEntity(request);
                    await _freightBidRepository.AddFreightBidAsync(entity);
                    await _dbContext.SaveChangesAsync(); // Ensures entity.Id is generated

                    // Step 2: Assign navigation collections using the now-persisted entity.Id
                    var useTags = request.UseTags?.Select(tag => ToFreightBidUseTagEntity(tag, entity.Id)).ToList()
                                   ?? new List<FreightBidUseTag>();

                    var helpOptions = request.HelpOptions?.Select(opt => ToFreightBidHelpOptionEntity(opt, entity.Id)).ToList()
                                       ?? new List<FreightBidHelpOption>();

                    // Step 3: Attach and persist navigation collections
                    entity.UseTags = useTags;
                    entity.HelpOptions = helpOptions;

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new FreightBidResponse {
                        Success = true,
                        Message = "Freight bid created.",
                        FreightBids = new List<FreightBidRequest> { request }
                    };
                } catch (ReferentialIntegrityException rie) {
                    await transaction.RollbackAsync();
                    _logger.LogError(rie, "Referential integrity error during freight bid creation.");
                    return ErrorResponse(rie.Message, rie.ErrorCode);
                } catch (FreightBidException fbx) {
                    await transaction.RollbackAsync();
                    _logger.LogError(fbx, "FreightBidException during freight bid creation.");
                    return ErrorResponse(fbx.Message, fbx.ErrorCode);
                } catch (DbUpdateException dbEx) {
                    await transaction.RollbackAsync();
                    _logger.LogError(dbEx, "DbUpdateException occurred when creating FreightBid.");
                    return ErrorResponse("Database update error during freight bid creation.", FreightBidErrorCodes.SaveFailed);
                } catch (Exception ex) {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Unhandled exception during freight bid creation.");
                    return ErrorResponse("Unexpected error occurred while creating freight bid.", FreightBidErrorCodes.Unknown);
                }
            });
        } catch (ValidationException vex) {
            return ErrorResponse($"Validation failed: {vex.Message}", FreightBidErrorCodes.ValidationError);
        } catch (Exception ex) {
            _logger.LogError(ex, "Unhandled error in CreateFreightBidAsync");
            return ErrorResponse("Unexpected error occurred while creating freight bid.", FreightBidErrorCodes.Unknown);
        }
    }

    public async Task<FreightBidResponse> UpdateFreightBidAsync(Guid id, FreightBidRequest request) {
        try {
            Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try {
                    // Step 1: Retrieve existing FreightBid
                    var existing = await _freightBidRepository.GetFreightBidByIdAsync(id);
                    if (existing == null)
                        return ErrorResponse("Freight bid not found.", FreightBidErrorCodes.NotFound);

                    // Step 2: Update core entity fields (not navigation properties)
                    var updated = ToFreightBidEntity(request);
                    updated.Id = id;
                    await _freightBidRepository.UpdateFreightBidAsync(updated);
                    await _dbContext.SaveChangesAsync();

                    // Step 3: Sync UseTags
                    var newUseTags = request.UseTags?
                        .Select(tag => ToFreightBidUseTagEntity(tag, id))
                        .ToList() ?? new();

                    _dbContext.FreightBidUseTags.RemoveRange(
                        existing.UseTags.Where(et => !newUseTags.Any(nt => nt.UseTagId == et.UseTagId))
                    );

                    var addedUseTags = newUseTags
                        .Where(nt => !existing.UseTags.Any(et => et.UseTagId == nt.UseTagId))
                        .ToList();

                    await _dbContext.FreightBidUseTags.AddRangeAsync(addedUseTags);

                    // Step 4: Sync HelpOptions
                    var newHelpOptions = request.HelpOptions?
                        .Select(opt => ToFreightBidHelpOptionEntity(opt, id))
                        .ToList() ?? new();

                    _dbContext.FreightBidHelpOptions.RemoveRange(
                        existing.HelpOptions.Where(eh => !newHelpOptions.Any(nh => nh.HelpOptionId == eh.HelpOptionId))
                    );

                    var addedHelpOptions = newHelpOptions
                        .Where(nh => !existing.HelpOptions.Any(eh => eh.HelpOptionId == nh.HelpOptionId))
                        .ToList();

                    await _dbContext.FreightBidHelpOptions.AddRangeAsync(addedHelpOptions);

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new FreightBidResponse {
                        Success = true,
                        Message = "Freight bid updated.",
                        FreightBids = new List<FreightBidRequest> { request }
                    };
                } catch (ReferentialIntegrityException rie) {
                    await transaction.RollbackAsync();
                    _logger.LogError(rie, "Referential integrity error during freight bid update.");
                    return ErrorResponse(rie.Message, rie.ErrorCode);
                } catch (FreightBidException fbx) {
                    await transaction.RollbackAsync();
                    _logger.LogError(fbx, "FreightBidException during freight bid update.");
                    return ErrorResponse(fbx.Message, fbx.ErrorCode);
                } catch (DbUpdateException dbEx) {
                    await transaction.RollbackAsync();
                    _logger.LogError(dbEx, "DbUpdateException occurred when updating FreightBid.");
                    return ErrorResponse("Database update error during freight bid update.", FreightBidErrorCodes.SaveFailed);
                } catch (Exception ex) {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Unhandled exception during freight bid update.");
                    return ErrorResponse("Unexpected error occurred while updating freight bid.", FreightBidErrorCodes.Unknown);
                }
            });
        } catch (ValidationException vex) {
            return ErrorResponse($"Validation failed: {vex.Message}", FreightBidErrorCodes.ValidationError);
        } catch (Exception ex) {
            _logger.LogError(ex, "Unhandled error in UpdateFreightBidAsync");
            return ErrorResponse("Unexpected error occurred while updating freight bid.", FreightBidErrorCodes.Unknown);
        }
    }

    public async Task<FreightBidResponse> DeleteFreightBidAsync(Guid id) {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () => {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try {
                var existing = await _freightBidRepository.GetFreightBidByIdAsync(id);
                if (existing == null)
                    return ErrorResponse("Freight bid not found.", ExceptionCodes.FreightBidErrorCodes.NotFound);

                await _freightBidRepository.DeleteFreightBidAsync(id);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new FreightBidResponse { Success = true, Message = "Freight bid deleted." };
            } catch (DbUpdateException dbEx) {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "DbUpdateException during FreightBid delete.");
                return ErrorResponse("Database update error during FreightBid deletion.",
                    ExceptionCodes.FreightBidErrorCodes.SaveFailed);
            } catch (Exception ex) {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unhandled exception during FreightBid delete.");
                return ErrorResponse("Unexpected error occurred while deleting FreightBid.",
                    ExceptionCodes.FreightBidErrorCodes.Unknown);
            }
        });
    }

    // --- FreightBid Details ---
    public async Task<FreightBidDetailsResponse> GetFreightBidDetailsAsync(Guid freightBidId) {
        try {
            var entity = await _freightBidRepository.GetFreightBidByIdAsync(freightBidId);
            if (entity == null) {
                return new FreightBidDetailsResponse {
                    Success = false,
                    Message = "Freight bid not found.",
                    ErrorCode = ExceptionCodes.FreightBidErrorCodes.NotFound,
                    FreightBidDetails = new()
                };
            }

            var driverBids = await _driverBidRepository.GetDriverBidsByFreightBidIdAsync(freightBidId) ?? new List<DriverBid>();

            var detail = ToFreightBidDetailsRequest(entity, driverBids);

            return new FreightBidDetailsResponse {
                Success = true,
                Message = "Freight bid details loaded.",
                FreightBidDetails = new List<FreightBidDetailsRequest> { detail }
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting freight bid details.");
            return new FreightBidDetailsResponse {
                Success = false,
                Message = "Error getting freight bid details.",
                ErrorCode = ExceptionCodes.FreightBidErrorCodes.Unknown,
                FreightBidDetails = new()
            };
        }
    }

    public async Task<FreightBidResponse> GetAllFreightBidsAsync() {
        try {
            var entities = await _freightBidRepository.GetAllFreightBidsAsync();
            return new FreightBidResponse {
                Success = true,
                FreightBids = entities.Select(ToFreightBidRequest).ToList()
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting all freight bids.");
            return ErrorResponse("Error getting all freight bids.", ExceptionCodes.FreightBidErrorCodes.Unknown);
        }
    }

    // --- UseTag Features ---

    public async Task<FreightBidUseTagResponse> CreateFreightBidUseTagAsync(FreightBidUseTagRequest request) {
        try {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try {
                    if (!await _dbContext.FreightBids.AnyAsync(fb => fb.Id == request.FreightBidId))
                        return ErrorUseTag("FreightBid does not exist.", ExceptionCodes.FreightBidErrorCodes.NotFound);

                    if (!await _dbContext.UseTags.AnyAsync(ut => ut.Id == request.UseTagId))
                        return ErrorUseTag("UseTag does not exist.", ExceptionCodes.FreightBidErrorCodes.UseTagNotFound);

                    var entity = ToFreightBidUseTagEntity(request);
                    await _freightBidRepository.AddFreightBidUseTagAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new FreightBidUseTagResponse {
                        Success = true,
                        Message = "FreightBidUseTag created.",
                        FreightBidUseTags = new List<FreightBidUseTagRequest> { request }
                    };
                } catch (DbUpdateException dbEx) {
                    await transaction.RollbackAsync();
                    _logger.LogError(dbEx, "DbUpdateException during FreightBidUseTag create.");
                    return ErrorUseTag("Database update error during FreightBidUseTag creation.",
                        ExceptionCodes.FreightBidErrorCodes.SaveFailed);
                } catch (Exception ex) {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Unhandled exception during FreightBidUseTag create.");
                    return ErrorUseTag("Unexpected error occurred while creating FreightBidUseTag.",
                        ExceptionCodes.FreightBidErrorCodes.Unknown);
                }
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Unhandled error in CreateFreightBidUseTagAsync");
            return ErrorUseTag("Unexpected error occurred while creating FreightBidUseTag.",
                ExceptionCodes.FreightBidErrorCodes.Unknown);
        }
    }

    private static FreightBidUseTag ToFreightBidUseTagEntity(FreightBidUseTagRequest request) => new() {
        FreightBidId = request.FreightBidId,
        UseTagId = request.UseTagId
    };

    public async Task<FreightBidUseTagResponse> DeleteFreightBidUseTagAsync(Guid id) {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try {
                var existing = await _freightBidRepository.GetFreightBidUseTagByIdAsync(id);
                if (existing == null)
                    return ErrorUseTag("FreightBidUseTag not found.", ExceptionCodes.FreightBidErrorCodes.NotFound);

                await _freightBidRepository.DeleteFreightBidUseTagAsync(id);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new FreightBidUseTagResponse { Success = true, Message = "FreightBidUseTag deleted." };
            } catch (DbUpdateException dbEx) {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "DbUpdateException during FreightBidUseTag delete.");
                return ErrorUseTag("Database update error during FreightBidUseTag deletion.",
                    ExceptionCodes.FreightBidErrorCodes.SaveFailed);
            } catch (Exception ex) {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unhandled exception during FreightBidUseTag delete.");
                return ErrorUseTag("Unexpected error occurred while deleting FreightBidUseTag.",
                    ExceptionCodes.FreightBidErrorCodes.Unknown);
            }
        });
    }

    public async Task<FreightBidUseTagResponse> GetFreightBidUseTagByIdAsync(Guid id) {
        try {
            var entity = await _freightBidRepository.GetFreightBidUseTagByIdAsync(id);
            if (entity == null)
                return ErrorUseTag("FreightBidUseTag not found.", ExceptionCodes.FreightBidErrorCodes.NotFound);

            return new FreightBidUseTagResponse {
                Success = true,
                FreightBidUseTags = new List<FreightBidUseTagRequest> { ToFreightBidUseTagRequest(entity) }
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting FreightBidUseTag by id.");
            return ErrorUseTag("Error getting FreightBidUseTag by id.", ExceptionCodes.FreightBidErrorCodes.Unknown);
        }
    }

    public async Task<FreightBidUseTagResponse> GetFreightBidUseTagsByFreightBidIdAsync(Guid freightBidId) {
        try {
            var entities = await _freightBidRepository.GetFreightBidUseTagsByFreightBidIdAsync(freightBidId);
            return new FreightBidUseTagResponse {
                Success = true,
                FreightBidUseTags = entities.Select(ToFreightBidUseTagRequest).ToList()
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting FreightBidUseTags by FreightBidId.");
            return ErrorUseTag("Error getting FreightBidUseTags by FreightBidId.",
                ExceptionCodes.FreightBidErrorCodes.Unknown);
        }
    }

    public async Task<FreightBidUseTagResponse> GetFreightBidUseTagsByUseTagIdAsync(Guid useTagId) {
        try {
            var entities = await _freightBidRepository.GetFreightBidUseTagsByUseTagIdAsync(useTagId);
            return new FreightBidUseTagResponse {
                Success = true,
                FreightBidUseTags = entities.Select(ToFreightBidUseTagRequest).ToList()
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting FreightBidUseTags by UseTagId.");
            return ErrorUseTag("Error getting FreightBidUseTags by UseTagId.",
                ExceptionCodes.FreightBidErrorCodes.Unknown);
        }
    }

    // --- Customer Bid History ---
    public async Task<BidHistoryResponse> GetBidHistoryAsync(Guid customerId) {
        try {
            var bids = await _freightBidRepository.GetBidHistoryByCustomerIdAsync(customerId);
            return new BidHistoryResponse {
                Success = true,
                BidHistorys = bids.Select(ToBidHistoryRequest).ToList()
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting bid history.");
            return new BidHistoryResponse {
                Success = false,
                Message = "Error getting bid history.",
                ErrorCode = ExceptionCodes.FreightBidErrorCodes.Unknown,
                BidHistorys = new()
            };
        }
    }

    // --- Find Drivers Status ---
    public async Task<FindDriversStatusResponse> GetFindDriversStatusAsync(Guid freightBidId) {
        try {
            var driverBids = await _driverBidRepository.GetDriverBidsByFreightBidIdAsync(freightBidId) ?? new List<DriverBid>();

            var statuses = driverBids.Select(ToFindDriversStatusRequest).ToList();

            return new FindDriversStatusResponse {
                Success = true,
                Message = statuses.Count == 0 ? "No driver bids found for this freight bid." : "Statuses found.",
                BidsStatuses = statuses,
                ErrorCode = null
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting find drivers status.");
            return new FindDriversStatusResponse {
                Success = false,
                Message = "Error getting find drivers status.",
                BidsStatuses = new List<FindDriversStatusRequest>(),
                ErrorCode = ExceptionCodes.FreightBidErrorCodes.Unknown
            };
        }
    }

    // --- Assign Driver ---
    public async Task<AssignDriverResponse> AssignDriverAsync(AssignDriverRequest request) {
        try {
            var bid = await _freightBidRepository.GetFreightBidByIdAsync(request.FreightBidId);
            if (bid == null) {
                return new AssignDriverResponse {
                    Success = false,
                    Message = "Freight bid not found.",
                    ErrorCode = ExceptionCodes.FreightBidErrorCodes.NotFound,
                    Assignments = new()
                };
            }

            var driverBid = await _driverBidRepository.GetDriverBidByIdAsync(request.DriverBidId);
            if (driverBid == null) {
                return new AssignDriverResponse {
                    Success = false,
                    Message = "Driver bid not found.",
                    ErrorCode = ExceptionCodes.FreightBidErrorCodes.DriverBidNotFound, // Use a specific code!
                    Assignments = new()
                };
            }

            var result = await _freightBidRepository.AssignDriverAsync(request.FreightBidId, request.DriverBidId);

            return new AssignDriverResponse {
                Success = result,
                Message = result ? "Driver assigned successfully." : "Failed to assign driver.",
                Assignments = new List<AssignDriverRequest> { request }
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error assigning driver.");
            return new AssignDriverResponse {
                Success = false,
                Message = "Error assigning driver.",
                ErrorCode = ExceptionCodes.FreightBidErrorCodes.Unknown,
                Assignments = new()
            };
        }
    }

    // --- Error helpers ---

    private static FreightBidUseTagResponse ErrorUseTag(string message, string? code = null)
        => new() {
            Success = false,
            Message = message,
            ErrorCode = code,
            FreightBidUseTags = new()
        };

    private static BidHistoryRequest ToBidHistoryRequest(FreightBid entity) {
        return new BidHistoryRequest(
            BidId: entity.Id,
            PlacedAt: entity.CreatedAt,
            PickupLocation: entity.PickupLocation,
            DeliveryLocation: entity.DeliveryLocation,
            VehicleType: entity.PreferredTruckType?.Name ?? "", // navigation property, safe
            Category: entity.TruckCategory?.Name ?? "",         // navigation property, safe
            Amount: 0, // You don't have an Amount property in FreightBid. Set properly if you do, or leave 0.
            Status: entity.Status.ToString()
        );
    }

    private static FreightBidDetailsRequest ToFreightBidDetailsRequest(
    FreightBid entity, IReadOnlyList<DriverBid> driverBids) {
        return new FreightBidDetailsRequest {
            FreightBidId = entity.Id,
            PickupLocation = entity.PickupLocation,
            DeliveryLocation = entity.DeliveryLocation,
            VehicleType = entity.PreferredTruckType?.Name ?? "",
            Category = entity.TruckCategory?.Name ?? "",
            // If you have logic for min/max, put it here (now left as null)
            EstimatedCostMin = null,
            EstimatedCostMax = null,
            Status = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt,
            DriverBids = driverBids?.Select(ToDriverBidRequest).ToList() ?? new()
        };
    }

    private static DriverBidRequest ToDriverBidRequest(DriverBid bid) {
        return new DriverBidRequest {
            // Fill this based on your DriverBidRequest DTO
            // Example:
            DriverBidId = bid.Id,
            FreightBidId = bid.FreightBidId,
            DriverId = bid.DriverProfileId,
            // Add other mapped properties here...
            // Amount, Status, SubmittedAt, etc.
        };
    }

    private static FindDriversStatusRequest ToFindDriversStatusRequest(DriverBid bid) {
        return new FindDriversStatusRequest(
            FreightBidId: bid.FreightBidId,
            DriversFound: true, // Each bid means at least one driver found
            TotalDriversFound: 1, // Or calculate based on your logic if you have grouping/aggregation
            RequestTime: bid.SubmittedAt,
            StatusMessage: bid.Status // Or customize this string as needed
        );
    }

    // FreightBid -> FreightBidRequest
    private static FreightBidRequest ToFreightBidRequest(FreightBid entity) {
        return new FreightBidRequest {
            PickupLocation = entity.PickupLocation,
            DeliveryLocation = entity.DeliveryLocation,
            PickupLat = entity.PickupLat,
            PickupLng = entity.PickupLng,
            PickupPlusCode = entity.PickupPlusCode,
            DeliveryLat = entity.DeliveryLat,
            DeliveryLng = entity.DeliveryLng,
            DeliveryPlusCode = entity.DeliveryPlusCode,

            // Now passing full entity
            TruckType = entity.PreferredTruckType,
            TruckCategory = entity.TruckCategory,
            BedType = entity.BedType,
            TruckMake = entity.TruckMake,
            TruckModel = entity.TruckModel,

            // UseTags: project out UseTag navigation
            UseTags = entity.UseTags?
                            .Where(x => x.UseTag != null)
                            .Select(x => x.UseTag!)
                            .ToList() ?? new(),

            // HelpOptions: if still name list, keep as is, otherwise project entity
            HelpOptions = entity.HelpOptions
                            .Where(h => h.HelpOption != null)
                            .Select(h => h.HelpOption!)
                            .ToList(),


            TravelWithPayload = entity.TravelWithPayload,
            TravelRequirement = entity.TravelRequirement,
            Insurance = entity.Insurance,
            Weight = entity.Weight,
            SpecialInstructions = entity.SpecialInstructions,
            ExpressService = entity.ExpressService,
            // If you want to set PaymentAccount, do it here if required
            // PaymentAccount   = entity.SelectedPaymentAccount, // remove or update if needed
        };
    }


    // FreightBidRequest -> FreightBid
    private static FreightBid ToFreightBidEntity(FreightBidRequest request) {
        return new FreightBid {
            // --- Identity & Foreign Keys ---
            CustomerProfileId = request.CustomerProfile?.Id ?? Guid.Empty,

            // --- Locations ---
            PickupLocation = request.PickupLocation,
            PickupLat = request.PickupLat,
            PickupLng = request.PickupLng,
            PickupPlusCode = request.PickupPlusCode,
            DeliveryLocation = request.DeliveryLocation,
            DeliveryLat = request.DeliveryLat,
            DeliveryLng = request.DeliveryLng,
            DeliveryPlusCode = request.DeliveryPlusCode,

            // --- Truck & Job ---
            PreferredTruckTypeId = request.TruckType?.Id ?? Guid.Empty,
            TruckCategoryId = request.TruckCategory?.Id,
            BedTypeId = request.BedType?.Id,
            TruckMakeId = request.TruckMake?.Id,
            TruckModelId = request.TruckModel?.Id,
            TravelWithPayload = request.TravelWithPayload,
            TravelRequirement = request.TravelRequirement,
            Insurance = request.Insurance,
            Weight = request.Weight,
            SpecialInstructions = request.SpecialInstructions,
            ExpressService = request.ExpressService,

            // --- Payment ---
            SelectedPaymentMethodId = request.PaymentAccount?.Id
        };
    }


    // FreightBidUseTagRequest -> FreightBidUseTag
    private static FreightBidUseTag ToFreightBidUseTagEntity(UseTag tag, Guid freightBidId) => new() {
        FreightBidId = freightBidId,
        UseTagId = tag.Id
    };

    // FreightBidUseTag -> FreightBidUseTagRequest
    private static FreightBidUseTagRequest ToFreightBidUseTagRequest(FreightBidUseTag entity) => new() {
        FreightBidId = entity.FreightBidId,
        UseTagId = entity.UseTagId
    };

    private static FreightBidHelpOption ToFreightBidHelpOptionEntity(HelpOption request, Guid freightBidId) => new() {
        FreightBidId = freightBidId,
        HelpOptionId = request.Id
    };

}
