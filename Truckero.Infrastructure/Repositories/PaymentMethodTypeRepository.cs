using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Truckero.Infrastructure.Repositories {
    public class PaymentMethodTypeRepository : IPaymentMethodTypeRepository {
        private readonly AppDbContext _context;

        public PaymentMethodTypeRepository(AppDbContext context) {
            _context = context;
        }

        // === ENTITY METHODS ONLY ===

        public async Task<List<PaymentMethodType>> GetAllPaymentMethodTypesAsync() {
            return await _context.PaymentMethodTypes
                .Where(p => p.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<PaymentMethodType>> GetPaymentMethodTypesByCountryAsync(string countryCode) {
            return await _context.PaymentMethodTypes
                .Where(p => p.IsActive && (p.CountryCode == countryCode || p.CountryCode == null))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PaymentMethodType?> GetPaymentMethodTypeByIdAsync(Guid id) {
            var entity = await _context.PaymentMethodTypes.FindAsync(id);
            return (entity != null && entity.IsActive) ? entity : null;
        }

        public async Task<PaymentMethodType> AddPaymentMethodTypeAsync(PaymentMethodType entity) {
            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            entity.IsActive = true;
            _context.PaymentMethodTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdatePaymentMethodTypeAsync(PaymentMethodType entity) {
            var existing = await _context.PaymentMethodTypes.FindAsync(entity.Id);
            if (existing == null || !existing.IsActive)
                throw new KeyNotFoundException("PaymentMethodType not found or inactive.");

            existing.Name = entity.Name;
            existing.Description = entity.Description;
            existing.CountryCode = entity.CountryCode;
            existing.IsForPayment = entity.IsForPayment;
            existing.IsForPayout = entity.IsForPayout;
            existing.IconUrl = entity.IconUrl;
            // Don't change IsActive here

            await _context.SaveChangesAsync();
        }

        public async Task InactivatePaymentMethodTypeAsync(Guid id) {
            var entity = await _context.PaymentMethodTypes.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("PaymentMethodType not found.");

            entity.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task DeletePaymentMethodTypeAsync(Guid id) {
            var entity = await _context.PaymentMethodTypes.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("PaymentMethodType not found.");

            _context.PaymentMethodTypes.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
