using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Truckero.Core.DTOs.PaymentMethodType;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;

namespace Truckero.Infrastructure.Services {
    /// <summary>
    /// Service for PaymentMethodType reference data (DTO-centric, uses entity repo).
    /// </summary>
    public class PaymentMethodTypeService : IPaymentMethodTypeService {
        private readonly IPaymentMethodTypeRepository _typeRepository;

        public PaymentMethodTypeService(IPaymentMethodTypeRepository typeRepository) {
            _typeRepository = typeRepository;
        }

        public async Task<List<PaymentMethodTypeRequest>> GetAllPaymentMethodTypesAsync() {
            var entities = await _typeRepository.GetAllPaymentMethodTypesAsync();
            return entities.Select(MapToDto).ToList();
        }

        public async Task<List<PaymentMethodTypeRequest>> GetPaymentMethodTypesByCountryAsync(string countryCode) {
            var entities = await _typeRepository.GetPaymentMethodTypesByCountryAsync(countryCode);
            return entities.Select(MapToDto).ToList();
        }

        public async Task<PaymentMethodTypeRequest?> GetPaymentMethodTypeByIdAsync(Guid id) {
            var entity = await _typeRepository.GetPaymentMethodTypeByIdAsync(id);
            return entity != null ? MapToDto(entity) : null;
        }

        public async Task<PaymentMethodTypeRequest> AddPaymentMethodTypeAsync(PaymentMethodTypeRequest dto) {
            var entity = MapToEntity(dto);
            var added = await _typeRepository.AddPaymentMethodTypeAsync(entity);
            return MapToDto(added);
        }

        public async Task UpdatePaymentMethodTypeAsync(PaymentMethodTypeRequest dto) {
            var entity = MapToEntity(dto);
            await _typeRepository.UpdatePaymentMethodTypeAsync(entity);
        }

        public async Task DeletePaymentMethodTypeAsync(Guid id) {
            await _typeRepository.DeletePaymentMethodTypeAsync(id);
        }

        // ---- Mapping helpers ----

        private static PaymentMethodTypeRequest MapToDto(PaymentMethodType entity) => new PaymentMethodTypeRequest {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CountryCode = entity.CountryCode,
            IsForPayment = entity.IsForPayment,
            IsForPayout = entity.IsForPayout,
            IconUrl = entity.IconUrl
        };

        private static PaymentMethodType MapToEntity(PaymentMethodTypeRequest dto) => new PaymentMethodType {
            Id = dto.Id,
            Name = dto.Name ?? "",
            Description = dto.Description,
            CountryCode = dto.CountryCode,
            IsForPayment = dto.IsForPayment,
            IsForPayout = dto.IsForPayout,
            IconUrl = dto.IconUrl,
            IsActive = true // Always true when adding/updating from UI
        };
    }
}
