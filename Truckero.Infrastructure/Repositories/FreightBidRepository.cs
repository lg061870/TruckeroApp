using Microsoft.EntityFrameworkCore;
using Truckero.Core.Constants;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories; 
public class FreightBidRepository : IFreightBidRepository {
    private readonly AppDbContext _context;

    public FreightBidRepository(AppDbContext context) {
        _context = context;
    }

    // FreightBid

    public async Task<FreightBid> GetFreightBidByIdAsync(Guid id) {
        try {
            var entity = await _context.FreightBids
                .Include(x => x.UseTags)
                    .ThenInclude(x => x.UseTag)
                .Include(x => x.DriverBids)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new FreightBidException("FreightBid not found.", ExceptionCodes.FreightBidErrorCodes.NotFound);

            return entity;
        } catch (FreightBidException) {
            throw;
        } catch (Exception ex) {
            throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
        }
    }

    public async Task<IReadOnlyList<FreightBid>> GetAllFreightBidsAsync() {
        try {
            return await _context.FreightBids
                .Include(x => x.UseTags)
                    .ThenInclude(x => x.UseTag)
                .Include(x => x.DriverBids)
                .ToListAsync();
        } catch (Exception ex) {
            throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
        }
    }

    public async Task<IReadOnlyList<FreightBid>> GetFreightBidsByCustomerIdAsync(Guid customerId) {
        try {
            return await _context.FreightBids
                .Where(x => x.CustomerId == customerId)
                .Include(x => x.UseTags)
                    .ThenInclude(x => x.UseTag)
                .Include(x => x.DriverBids)
                .ToListAsync();
        } catch (Exception ex) {
            throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
        }
    }

    public async Task<FreightBid> AddFreightBidAsync(FreightBid entity) {
        try {
            // Foreign key checks
            if (!await _context.Users.AnyAsync(u => u.Id == entity.CustomerId))
                throw new FreightBidException("CustomerId does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

            if (entity.PreferredTruckTypeId.HasValue &&
                !await _context.TruckTypes.AnyAsync(tt => tt.Id == entity.PreferredTruckTypeId.Value))
                throw new FreightBidException("PreferredTruckTypeId does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

            if (entity.AssignedTruckId.HasValue &&
                !await _context.Trucks.AnyAsync(t => t.Id == entity.AssignedTruckId.Value))
                throw new FreightBidException("AssignedTruckId does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

            if (entity.AssignedDriverId.HasValue &&
                !await _context.Users.AnyAsync(u => u.Id == entity.AssignedDriverId.Value))
                throw new FreightBidException("AssignedDriverId does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

            if (entity.SelectedPaymentMethodId.HasValue &&
                !await _context.PaymentAccounts.AnyAsync(p => p.Id == entity.SelectedPaymentMethodId.Value))
                throw new FreightBidException("SelectedPaymentMethodId does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

            await _context.FreightBids.AddAsync(entity);
            var saved = await _context.SaveChangesAsync();
            if (saved < 1)
                throw new FreightBidException("Save failed.", ExceptionCodes.FreightBidErrorCodes.SaveFailed);

            return entity;
        } catch (FreightBidException) {
            throw;
        } catch (Exception ex) {
            throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
        }
    }

    public async Task<FreightBid> UpdateFreightBidAsync(FreightBid entity) {
        try {
            var existing = await _context.FreightBids.FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (existing == null)
                throw new FreightBidException("FreightBid not found.", ExceptionCodes.FreightBidErrorCodes.NotFound);

            // Foreign key checks
            if (!await _context.Users.AnyAsync(u => u.Id == entity.CustomerId))
                throw new FreightBidException("CustomerId does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

            if (entity.PreferredTruckTypeId.HasValue &&
                !await _context.TruckTypes.AnyAsync(tt => tt.Id == entity.PreferredTruckTypeId.Value))
                throw new FreightBidException("PreferredTruckTypeId does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

            if (entity.AssignedTruckId.HasValue &&
                !await _context.Trucks.AnyAsync(t => t.Id == entity.AssignedTruckId.Value))
                throw new FreightBidException("AssignedTruckId does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

            if (entity.AssignedDriverId.HasValue &&
                !await _context.Users.AnyAsync(u => u.Id == entity.AssignedDriverId.Value))
                throw new FreightBidException("AssignedDriverId does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

            if (entity.SelectedPaymentMethodId.HasValue &&
                !await _context.PaymentAccounts.AnyAsync(p => p.Id == entity.SelectedPaymentMethodId.Value))
                throw new FreightBidException("SelectedPaymentMethodId does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

            // Copy fields (excluding navigation)
            existing.CustomerId = entity.CustomerId;
            existing.PickupLocation = entity.PickupLocation;
            existing.DeliveryLocation = entity.DeliveryLocation;
            existing.PreferredTruckTypeId = entity.PreferredTruckTypeId;
            existing.Weight = entity.Weight;
            existing.SpecialInstructions = entity.SpecialInstructions;
            existing.Insurance = entity.Insurance;
            existing.TravelWithPayload = entity.TravelWithPayload;
            existing.TravelRequirement = entity.TravelRequirement;
            existing.ExpressService = entity.ExpressService;
            existing.CreatedAt = entity.CreatedAt;
            existing.SelectedPaymentMethodId = entity.SelectedPaymentMethodId;
            existing.Status = entity.Status;
            existing.AssignedTruckId = entity.AssignedTruckId;
            existing.AssignedDriverId = entity.AssignedDriverId;

            _context.FreightBids.Update(existing);
            var saved = await _context.SaveChangesAsync();
            if (saved < 1)
                throw new FreightBidException("Save failed.", ExceptionCodes.FreightBidErrorCodes.SaveFailed);

            return existing;
        } catch (FreightBidException) {
            throw;
        } catch (Exception ex) {
            throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
        }
    }

    public async Task DeleteFreightBidAsync(Guid id) {
        try {
            var entity = await _context.FreightBids.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new FreightBidException("FreightBid not found.", ExceptionCodes.FreightBidErrorCodes.NotFound);

            _context.FreightBids.Remove(entity);
            var saved = await _context.SaveChangesAsync();
            if (saved < 1)
                throw new FreightBidException("Save failed.", ExceptionCodes.FreightBidErrorCodes.SaveFailed);
        } catch (FreightBidException) {
            throw;
        } catch (Exception ex) {
            throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
        }
    }

    // FreightBidUseTag

    public async Task<FreightBidUseTag> GetFreightBidUseTagByIdAsync(Guid id) {
        try {
            var entity = await _context.FreightBidUseTags
                .Include(x => x.FreightBid)
                .Include(x => x.UseTag)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new FreightBidException("FreightBidUseTag not found.", ExceptionCodes.FreightBidErrorCodes.NotFound);

            return entity;
        } catch (FreightBidException) {
            throw;
        } catch (Exception ex) {
            throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
        }
    }

    public async Task<IReadOnlyList<FreightBidUseTag>> GetFreightBidUseTagsByFreightBidIdAsync(Guid freightBidId) {
        try {
            return await _context.FreightBidUseTags
                .Where(x => x.FreightBidId == freightBidId)
                .Include(x => x.UseTag)
                .ToListAsync();
        } catch (Exception ex) {
            throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
        }
    }

    public async Task<IReadOnlyList<FreightBidUseTag>> GetFreightBidUseTagsByUseTagIdAsync(Guid useTagId) {
        try {
            return await _context.FreightBidUseTags
                .Where(x => x.UseTagId == useTagId)
                .Include(x => x.FreightBid)
                .ToListAsync();
        } catch (Exception ex) {
            throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
        }
    }

    public async Task<FreightBidUseTag> AddFreightBidUseTagAsync(FreightBidUseTag entity) {
        try {
            if (!await _context.FreightBids.AnyAsync(fb => fb.Id == entity.FreightBidId))
                throw new FreightBidException("FreightBidId does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

            if (!await _context.UseTags.AnyAsync(ut => ut.Id == entity.UseTagId))
                throw new FreightBidException("UseTagId does not exist.", ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound);

            await _context.FreightBidUseTags.AddAsync(entity);
            var saved = await _context.SaveChangesAsync();
            if (saved < 1)
                throw new FreightBidException("Save failed.", ExceptionCodes.FreightBidErrorCodes.SaveFailed);

            return entity;
        } catch (FreightBidException) {
            throw;
        } catch (Exception ex) {
            throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
        }
    }

    public async Task DeleteFreightBidUseTagAsync(Guid id) {
        try {
            var entity = await _context.FreightBidUseTags.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new FreightBidException("FreightBidUseTag not found.", ExceptionCodes.FreightBidErrorCodes.NotFound);

            _context.FreightBidUseTags.Remove(entity);
            var saved = await _context.SaveChangesAsync();
            if (saved < 1)
                throw new FreightBidException("Save failed.", ExceptionCodes.FreightBidErrorCodes.SaveFailed);
        } catch (FreightBidException) {
            throw;
        } catch (Exception ex) {
            throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
        }
    }
}
