using Microsoft.EntityFrameworkCore;
using Truckero.Core.Constants;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Truckero.Infrastructure.Repositories {
    public class FreightBidRepository : IFreightBidRepository {
        private readonly AppDbContext _context;

        public FreightBidRepository(AppDbContext context) {
            _context = context;
        }

        // ========== FreightBid ==========

        public async Task<FreightBid> GetFreightBidByIdAsync(Guid id) {
            try {
                var entity = await _context.FreightBids
                    .Include(x => x.UseTags).ThenInclude(x => x.UseTag)
                    .Include(x => x.HelpOptions).ThenInclude(x => x.HelpOption)
                    .Include(x => x.DriverBids)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null)
                    throw new FreightBidException("FreightBid not found.", ExceptionCodes.FreightBidErrorCodes.NotFound);

                return entity;
            } catch (FreightBidException) { throw; } catch (Exception ex) {
                throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
            }
        }

        public async Task<IReadOnlyList<FreightBid>> GetAllFreightBidsAsync() {
            try {
                return await _context.FreightBids
                    .Include(x => x.UseTags).ThenInclude(x => x.UseTag)
                    .Include(x => x.HelpOptions).ThenInclude(x => x.HelpOption)
                    .Include(x => x.DriverBids)
                    .ToListAsync();
            } catch (Exception ex) {
                throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
            }
        }

        public async Task<IReadOnlyList<FreightBid>> GetFreightBidsByCustomerIdAsync(Guid customerId) {
            try {
                return await _context.FreightBids
                    .Where(x => x.CustomerProfileId == customerId)
                    .Include(x => x.UseTags).ThenInclude(x => x.UseTag)
                    .Include(x => x.HelpOptions).ThenInclude(x => x.HelpOption)
                    .Include(x => x.DriverBids)
                    .ToListAsync();
            } catch (Exception ex) {
                throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
            }
        }

        public async Task<FreightBid> AddFreightBidAsync(FreightBid entity) {
            try {
                CustomerProfile? customerProfile = await _context.CustomerProfiles.Where(cp => cp.Id == entity.CustomerProfileId).FirstOrDefaultAsync();

                if (customerProfile == null)
                    throw new ReferentialIntegrityException("Customer profile does not exist.", ExceptionCodes.FreightBidErrorCodes.CustomerNotFound);

                if (!await _context.TruckTypes.AnyAsync(tt => tt.Id == entity.PreferredTruckTypeId))
                    throw new ReferentialIntegrityException("Preferred truck type does not exist.", ExceptionCodes.FreightBidErrorCodes.PreferredTruckTypeNotFound);

                if (entity.AssignedTruckId.HasValue &&
                    !await _context.Trucks.AnyAsync(t => t.Id == entity.AssignedTruckId.Value))
                    throw new ReferentialIntegrityException("Assigned truck does not exist.", ExceptionCodes.FreightBidErrorCodes.AssignedTruckNotFound);

                if (entity.AssignedDriverId.HasValue &&
                    !await _context.Users.AnyAsync(u => u.Id == entity.AssignedDriverId.Value))
                    throw new ReferentialIntegrityException("Assigned driver does not exist.", ExceptionCodes.FreightBidErrorCodes.AssignedDriverNotFound);

                if (entity.SelectedPaymentMethodId.HasValue &&
                    !await _context.PaymentAccounts.AnyAsync(p => p.Id == entity.SelectedPaymentMethodId.Value))
                    throw new ReferentialIntegrityException("Selected payment method does not exist.", ExceptionCodes.FreightBidErrorCodes.PaymentMethodNotFound);

                await _context.FreightBids.AddAsync(entity);
                var saved = await _context.SaveChangesAsync();
                if (saved < 1)
                    throw new FreightBidException("Save failed.", ExceptionCodes.FreightBidErrorCodes.SaveFailed);

                return entity;
            } catch (ReferentialIntegrityException) { throw; } // Bubble up
            catch (FreightBidException) { throw; }           // Bubble up
            catch (Exception ex) {
                throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
            }
        }

        public async Task<FreightBid> UpdateFreightBidAsync(FreightBid entity) {
            try {
                var existing = await _context.FreightBids.FirstOrDefaultAsync(x => x.Id == entity.Id);
                if (existing == null)
                    throw new FreightBidException("FreightBid not found.", ExceptionCodes.FreightBidErrorCodes.NotFound);

                // Referential integrity checks (use ReferentialIntegrityException)
                if (!await _context.Users.AnyAsync(u => u.Id == entity.CustomerProfileId))
                    throw new ReferentialIntegrityException("Customer profile does not exist.", ExceptionCodes.FreightBidErrorCodes.CustomerNotFound);

                if (!await _context.TruckTypes.AnyAsync(tt => tt.Id == entity.PreferredTruckTypeId))
                    throw new ReferentialIntegrityException("Preferred truck type does not exist.", ExceptionCodes.FreightBidErrorCodes.PreferredTruckTypeNotFound);

                if (entity.AssignedTruckId.HasValue &&
                    !await _context.Trucks.AnyAsync(t => t.Id == entity.AssignedTruckId.Value))
                    throw new ReferentialIntegrityException("Assigned truck does not exist.", ExceptionCodes.FreightBidErrorCodes.AssignedTruckNotFound);

                if (entity.AssignedDriverId.HasValue &&
                    !await _context.Users.AnyAsync(u => u.Id == entity.AssignedDriverId.Value))
                    throw new ReferentialIntegrityException("Assigned driver does not exist.", ExceptionCodes.FreightBidErrorCodes.AssignedDriverNotFound);

                if (entity.SelectedPaymentMethodId.HasValue &&
                    !await _context.PaymentAccounts.AnyAsync(p => p.Id == entity.SelectedPaymentMethodId.Value))
                    throw new ReferentialIntegrityException("Selected payment method does not exist.", ExceptionCodes.FreightBidErrorCodes.PaymentMethodNotFound);

                // Update properties
                existing.CustomerProfileId = entity.CustomerProfileId;
                existing.PickupLocation = entity.PickupLocation;
                existing.PickupLat = entity.PickupLat;
                existing.PickupLng = entity.PickupLng;
                existing.PickupPlusCode = entity.PickupPlusCode;
                existing.DeliveryLocation = entity.DeliveryLocation;
                existing.DeliveryLat = entity.DeliveryLat;
                existing.DeliveryLng = entity.DeliveryLng;
                existing.DeliveryPlusCode = entity.DeliveryPlusCode;
                existing.PreferredTruckTypeId = entity.PreferredTruckTypeId;
                existing.TruckCategoryId = entity.TruckCategoryId;
                existing.BedTypeId = entity.BedTypeId;
                existing.TruckMakeId = entity.TruckMakeId;
                existing.TruckModelId = entity.TruckModelId;
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
            } catch (ReferentialIntegrityException) { throw; } catch (FreightBidException) { throw; } catch (Exception ex) {
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
            } catch (FreightBidException) { throw; } catch (Exception ex) {
                throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
            }
        }

        // ========== FreightBidUseTag ==========

        public async Task<FreightBidUseTag> GetFreightBidUseTagByIdAsync(Guid id) {
            try {
                var entity = await _context.FreightBidUseTags
                    .Include(x => x.FreightBid)
                    .Include(x => x.UseTag)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null)
                    throw new FreightBidException("FreightBidUseTag not found.", ExceptionCodes.FreightBidErrorCodes.FreightBidUseTagNotFound);

                return entity;
            } catch (FreightBidException) { throw; } catch (Exception ex) {
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
                    throw new FreightBidException("FreightBid does not exist.", ExceptionCodes.FreightBidErrorCodes.NotFound);

                if (!await _context.UseTags.AnyAsync(ut => ut.Id == entity.UseTagId))
                    throw new FreightBidException("UseTag does not exist.", ExceptionCodes.FreightBidErrorCodes.UseTagNotFound);

                await _context.FreightBidUseTags.AddAsync(entity);
                var saved = await _context.SaveChangesAsync();
                if (saved < 1)
                    throw new FreightBidException("Save failed.", ExceptionCodes.FreightBidErrorCodes.SaveFailed);

                return entity;
            } catch (FreightBidException) { throw; } catch (Exception ex) {
                throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
            }
        }

        public async Task DeleteFreightBidUseTagAsync(Guid id) {
            try {
                var entity = await _context.FreightBidUseTags.FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                    throw new FreightBidException("FreightBidUseTag not found.", ExceptionCodes.FreightBidErrorCodes.FreightBidUseTagNotFound);

                _context.FreightBidUseTags.Remove(entity);
                var saved = await _context.SaveChangesAsync();
                if (saved < 1)
                    throw new FreightBidException("Save failed.", ExceptionCodes.FreightBidErrorCodes.SaveFailed);
            } catch (FreightBidException) { throw; } catch (Exception ex) {
                throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
            }
        }

        // ========== Optional: Bid History ==========

        public async Task<IReadOnlyList<FreightBid>> GetBidHistoryByCustomerIdAsync(Guid customerId) {
            try {
                return await _context.FreightBids
                    .Where(x => x.CustomerProfileId == customerId)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            } catch (Exception ex) {
                throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
            }
        }

        public async Task<bool> AssignDriverAsync(Guid freightBidId, Guid driverBidId) {
            try {
                var freightBid = await _context.FreightBids.FirstOrDefaultAsync(x => x.Id == freightBidId);
                if (freightBid == null)
                    throw new FreightBidException("FreightBid not found.", ExceptionCodes.FreightBidErrorCodes.NotFound);

                var driverBid = await _context.DriverBids.FirstOrDefaultAsync(x => x.Id == driverBidId && x.FreightBidId == freightBidId);
                if (driverBid == null)
                    throw new FreightBidException("DriverBid not found for this FreightBid.", ExceptionCodes.FreightBidErrorCodes.AssignedDriverNotFound);

                // Mark the FreightBid as assigned
                freightBid.AssignedDriverId = driverBid.DriverProfileId;
                freightBid.AssignedTruckId = driverBid.TruckId;
                freightBid.Status = FreightBidStatus.Assigned;
                await _context.SaveChangesAsync();

                return true;
            } catch (FreightBidException) { throw; } catch (Exception ex) {
                throw new FreightBidException("Unknown error occurred.", ExceptionCodes.FreightBidErrorCodes.Unknown, ex);
            }
        }
    }
}
