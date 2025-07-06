using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Truckero.Core.Constants;
using Truckero.Core.DTOs.CustomerFlow;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Services;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Services;

public class FreightBidService : IFreightBidService {
    private readonly AppDbContext _dbContext;
    private readonly IFreightBidRepository _freightBidRepository;
    private readonly ILogger<FreightBidService> _logger;

    public FreightBidService(
        AppDbContext dbContext,
        IFreightBidRepository freightBidRepository,
        ILogger<FreightBidService> logger) {
        _dbContext = dbContext;
        _freightBidRepository = freightBidRepository;
        _logger = logger;
    }

    // ---- FreightBid ----

    public async Task<FreightBidResponse> CreateFreightBidAsync(FreightBidRequest request) {
        try {
            Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () => {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try {
                    // Foreign key validations
                    if (!await _dbContext.TruckTypes.AnyAsync(tt => tt.Id == request.TruckTypeId))
                        return ErrorResponse("TruckType does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

                    if (request.TruckCategoryId.HasValue &&
                        !await _dbContext.TruckCategories.AnyAsync(tc => tc.Id == request.TruckCategoryId.Value))
                        return ErrorResponse("TruckCategory does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

                    if (request.BedTypeId.HasValue &&
                        !await _dbContext.BedTypes.AnyAsync(b => b.Id == request.BedTypeId.Value))
                        return ErrorResponse("BedType does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

                    if (!string.IsNullOrEmpty(request.PaymentMethodId) &&
                        !Guid.TryParse(request.PaymentMethodId, out var paymentGuid))
                        return ErrorResponse("PaymentMethodId is not a valid GUID.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

                    if (!string.IsNullOrEmpty(request.PaymentMethodId) &&
                        !await _dbContext.PaymentAccounts.AnyAsync(p => p.Id == Guid.Parse(request.PaymentMethodId)))
                        return ErrorResponse("PaymentMethodId does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

                    foreach (var tagId in request.UseTagIds) {
                        if (!await _dbContext.UseTags.AnyAsync(u => u.Id == tagId))
                            return ErrorResponse($"UseTagId '{tagId}' does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);
                    }

                    var entity = ToFreightBidEntity(request);
                    await _freightBidRepository.AddFreightBidAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new FreightBidResponse {
                        Success = true,
                        Message = "Freight bid created.",
                        FreightBids = new List<FreightBidRequest> { request }
                    };
                } catch (DbUpdateException dbEx) {
                    await transaction.RollbackAsync();
                    _logger.LogError(dbEx, "DbUpdateException occurred when creating FreightBid.");
                    return ErrorResponse("Database update error during freight bid creation.",
                        ExceptionCodes.FreightBidErrorCodes.SaveFailed);
                } catch (Exception ex) {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Unhandled exception during freight bid creation.");
                    return ErrorResponse("Unexpected error occurred while creating freight bid.",
                        ExceptionCodes.FreightBidErrorCodes.Unknown);
                }
            });
        } catch (ValidationException vex) {
            return ErrorResponse($"Validation failed: {vex.Message}", ExceptionCodes.FreightBidErrorCodes.ValidationError);
        } catch (Exception ex) {
            _logger.LogError(ex, "Unhandled error in CreateFreightBidAsync");
            return ErrorResponse("Unexpected error occurred while creating freight bid.",
                ExceptionCodes.FreightBidErrorCodes.Unknown);
        }
    }

    public async Task<FreightBidResponse> UpdateFreightBidAsync(Guid id, FreightBidRequest request) {
        try {
            Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () => {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try {
                    var existing = await _freightBidRepository.GetFreightBidByIdAsync(id);
                    if (existing == null)
                        return ErrorResponse("Freight bid not found.", ExceptionCodes.FreightBidErrorCodes.NotFound);

                    // FK validation & logic as above

                    var entity = ToFreightBidEntity(request);
                    entity.Id = id;
                    await _freightBidRepository.UpdateFreightBidAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new FreightBidResponse {
                        Success = true,
                        Message = "Freight bid updated.",
                        FreightBids = new List<FreightBidRequest> { request }
                    };
                } catch (DbUpdateException dbEx) {
                    await transaction.RollbackAsync();
                    _logger.LogError(dbEx, "DbUpdateException occurred when updating FreightBid.");
                    return ErrorResponse("Database update error during freight bid update.",
                        ExceptionCodes.FreightBidErrorCodes.SaveFailed);
                } catch (Exception ex) {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Unhandled exception during freight bid update.");
                    return ErrorResponse("Unexpected error occurred while updating freight bid.",
                        ExceptionCodes.FreightBidErrorCodes.Unknown);
                }
            });
        } catch (ValidationException vex) {
            return ErrorResponse($"Validation failed: {vex.Message}", ExceptionCodes.FreightBidErrorCodes.ValidationError);
        } catch (Exception ex) {
            _logger.LogError(ex, "Unhandled error in UpdateFreightBidAsync");
            return ErrorResponse("Unexpected error occurred while updating freight bid.",
                ExceptionCodes.FreightBidErrorCodes.Unknown);
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

    public async Task<FreightBidResponse> GetFreightBidByIdAsync(Guid id) {
        try {
            var entity = await _freightBidRepository.GetFreightBidByIdAsync(id);
            if (entity == null)
                return ErrorResponse("Freight bid not found.", ExceptionCodes.FreightBidErrorCodes.NotFound);

            return new FreightBidResponse {
                Success = true,
                FreightBids = new List<FreightBidRequest> { ToFreightBidRequest(entity) }
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting freight bid by id.");
            return ErrorResponse("Error getting freight bid by id.", ExceptionCodes.FreightBidErrorCodes.Unknown);
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

    public async Task<FreightBidResponse> GetFreightBidsByCustomerIdAsync(Guid customerId) {
        try {
            var entities = await _freightBidRepository.GetFreightBidsByCustomerIdAsync(customerId);
            return new FreightBidResponse {
                Success = true,
                FreightBids = entities.Select(ToFreightBidRequest).ToList()
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting freight bids by customer id.");
            return ErrorResponse("Error getting freight bids by customer id.", ExceptionCodes.FreightBidErrorCodes.Unknown);
        }
    }

    // ---- FreightBidUseTag ----

    public async Task<FreightBidUseTagResponse> CreateFreightBidUseTagAsync(FreightBidUseTagRequest request) {
        try {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () => {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try {
                    // FK validation
                    if (!await _dbContext.FreightBids.AnyAsync(fb => fb.Id == request.FreightBidId))
                        return ErrorUseTag("FreightBid does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

                    if (!await _dbContext.UseTags.AnyAsync(ut => ut.Id == request.UseTagId))
                        return ErrorUseTag("UseTag does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

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

    public async Task<FreightBidUseTagResponse> DeleteFreightBidUseTagAsync(Guid id) {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () => {
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

    // --- Mapping Helpers ---

    private static FreightBidRequest ToFreightBidRequest(FreightBid entity) {
        return new FreightBidRequest {
            PickupLocation = entity.PickupLocation,
            DeliveryLocation = entity.DeliveryLocation,
            TruckTypeId = entity.PreferredTruckTypeId ?? Guid.Empty,
            TruckCategoryId = entity.AssignedTruckId, // Update with correct mapping if needed
            BedTypeId = null, // Map if present
            UseTagIds = entity.UseTags?.Select(t => t.UseTagId).ToList() ?? new List<Guid>(),
            PayloadType = "", // Map from entity if available
            OtherPayload = null, // Map if available
            HelpOptions = new List<string>(), // Map if available
            TravelWithPayload = entity.TravelWithPayload,
            TravelRequirement = entity.TravelRequirement,
            Insurance = entity.Insurance,
            Weight = entity.Weight,
            SpecialInstructions = entity.SpecialInstructions,
            PaymentMethodId = entity.SelectedPaymentMethodId?.ToString(),
            ExpressService = entity.ExpressService
        };
    }

    private static FreightBid ToFreightBidEntity(FreightBidRequest request) {
        return new FreightBid {
            PickupLocation = request.PickupLocation,
            DeliveryLocation = request.DeliveryLocation,
            PreferredTruckTypeId = request.TruckTypeId,
            // Assign TruckCategoryId, BedTypeId, etc., if your entity supports it
            Weight = request.Weight,
            SpecialInstructions = request.SpecialInstructions,
            Insurance = request.Insurance,
            TravelWithPayload = request.TravelWithPayload,
            TravelRequirement = request.TravelRequirement,
            ExpressService = request.ExpressService,
            SelectedPaymentMethodId = !string.IsNullOrEmpty(request.PaymentMethodId)
                ? Guid.Parse(request.PaymentMethodId)
                : (Guid?)null
            // UseTags handled separately
        };
    }

    private static FreightBidUseTagRequest ToFreightBidUseTagRequest(FreightBidUseTag entity) {
        return new FreightBidUseTagRequest {
            FreightBidId = entity.FreightBidId,
            UseTagId = entity.UseTagId
        };
    }

    private static FreightBidUseTag ToFreightBidUseTagEntity(FreightBidUseTagRequest request) {
        return new FreightBidUseTag {
            FreightBidId = request.FreightBidId,
            UseTagId = request.UseTagId
        };
    }

    // --- Error helpers ---

    private static FreightBidResponse ErrorResponse(string message, string? code = null)
        => new() {
            Success = false,
            Message = message,
            ErrorCode = code,
            FreightBids = new()
        };

    private static FreightBidUseTagResponse ErrorUseTag(string message, string? code = null)
        => new() {
            Success = false,
            Message = message,
            ErrorCode = code,
            FreightBidUseTags = new()
        };
}
