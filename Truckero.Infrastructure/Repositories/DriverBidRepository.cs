using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Constants;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories;

public class DriverBidRepository : IDriverBidRepository {
    private readonly AppDbContext _context;

    public DriverBidRepository(AppDbContext context) {
        _context = context;
    }

    public async Task<DriverBid> GetDriverBidByIdAsync(Guid id) {
        try {
            var entity = await _context.DriverBids
                .Include(db => db.FreightBid)
                .FirstOrDefaultAsync(db => db.Id == id);

            if (entity == null)
                throw new DriverBidException("DriverBid not found.", ExceptionCodes.DriverBidErrorCodes.NotFound);

            return entity;
        } catch (DriverBidException) {
            throw;
        } catch (Exception ex) {
            throw new DriverBidException("Unknown error occurred.", ExceptionCodes.DriverBidErrorCodes.Unknown, ex);
        }
    }

    public async Task<IReadOnlyList<DriverBid>> GetDriverBidsByFreightBidIdAsync(Guid freightBidId) {
        try {
            return await _context.DriverBids
                .Where(db => db.FreightBidId == freightBidId)
                .Include(db => db.FreightBid)
                .ToListAsync();
        } catch (Exception ex) {
            throw new DriverBidException("Unknown error occurred.", ExceptionCodes.DriverBidErrorCodes.Unknown, ex);
        }
    }

    public async Task<IReadOnlyList<DriverBid>> GetDriverBidsByDriverIdAsync(Guid driverId) {
        try {
            return await _context.DriverBids
                .Where(db => db.DriverId == driverId)
                .Include(db => db.FreightBid)
                .ToListAsync();
        } catch (Exception ex) {
            throw new DriverBidException("Unknown error occurred.", ExceptionCodes.DriverBidErrorCodes.Unknown, ex);
        }
    }

    public async Task<DriverBid> AddDriverBidAsync(DriverBid entity) {
        try {
            // Foreign key checks
            if (!await _context.FreightBids.AnyAsync(fb => fb.Id == entity.FreightBidId))
                throw new DriverBidException("FreightBidId does not exist.", ExceptionCodes.DriverBidErrorCodes.FreightBidNotFound);

            if (!await _context.Users.AnyAsync(u => u.Id == entity.DriverId))
                throw new DriverBidException("DriverId does not exist.", ExceptionCodes.DriverBidErrorCodes.DriverNotFound);

            if (!await _context.Trucks.AnyAsync(t => t.Id == entity.TruckId))
                throw new DriverBidException("TruckId does not exist.", ExceptionCodes.DriverBidErrorCodes.TruckNotFound);

            await _context.DriverBids.AddAsync(entity);
            var saved = await _context.SaveChangesAsync();
            if (saved < 1)
                throw new DriverBidException("Save failed.", ExceptionCodes.DriverBidErrorCodes.SaveFailed);

            return entity;
        } catch (DriverBidException) {
            throw;
        } catch (Exception ex) {
            throw new DriverBidException("Unknown error occurred.", ExceptionCodes.DriverBidErrorCodes.Unknown, ex);
        }
    }

    public async Task<DriverBid> UpdateDriverBidAsync(DriverBid entity) {
        try {
            var existing = await _context.DriverBids.FirstOrDefaultAsync(db => db.Id == entity.Id);
            if (existing == null)
                throw new DriverBidException("DriverBid not found.", ExceptionCodes.DriverBidErrorCodes.NotFound);

            // Foreign key checks
            if (!await _context.FreightBids.AnyAsync(fb => fb.Id == entity.FreightBidId))
                throw new DriverBidException("FreightBidId does not exist.", ExceptionCodes.DriverBidErrorCodes.FreightBidNotFound);

            if (!await _context.Users.AnyAsync(u => u.Id == entity.DriverId))
                throw new DriverBidException("DriverId does not exist.", ExceptionCodes.DriverBidErrorCodes.DriverNotFound);

            if (!await _context.Trucks.AnyAsync(t => t.Id == entity.TruckId))
                throw new DriverBidException("TruckId does not exist.", ExceptionCodes.DriverBidErrorCodes.TruckNotFound);

            // Update fields
            existing.FreightBidId = entity.FreightBidId;
            existing.DriverId = entity.DriverId;
            existing.TruckId = entity.TruckId;
            existing.OfferAmount = entity.OfferAmount;
            existing.Message = entity.Message;
            existing.SubmittedAt = entity.SubmittedAt;
            existing.Status = entity.Status;

            _context.DriverBids.Update(existing);
            var saved = await _context.SaveChangesAsync();
            if (saved < 1)
                throw new DriverBidException("Save failed.", ExceptionCodes.DriverBidErrorCodes.SaveFailed);

            return existing;
        } catch (DriverBidException) {
            throw;
        } catch (Exception ex) {
            throw new DriverBidException("Unknown error occurred.", ExceptionCodes.DriverBidErrorCodes.Unknown, ex);
        }
    }

    public async Task DeleteDriverBidAsync(Guid id) {
        try {
            var entity = await _context.DriverBids.FirstOrDefaultAsync(db => db.Id == id);
            if (entity == null)
                throw new DriverBidException("DriverBid not found.", ExceptionCodes.DriverBidErrorCodes.NotFound);

            _context.DriverBids.Remove(entity);
            var saved = await _context.SaveChangesAsync();
            if (saved < 1)
                throw new DriverBidException("Save failed.", ExceptionCodes.DriverBidErrorCodes.SaveFailed);
        } catch (DriverBidException) {
            throw;
        } catch (Exception ex) {
            throw new DriverBidException("Unknown error occurred.", ExceptionCodes.DriverBidErrorCodes.Unknown, ex);
        }
    }
}
