using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Truckero.Core.Constants;
using Truckero.Core.DTOs.PaymentMethodType;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;

namespace Truckero.Infrastructure.Services; 
/// <summary>
/// Service for PaymentMethodType reference data (DTO-centric, uses entity repo).
/// </summary>
public class PaymentMethodTypeService : IPaymentMethodTypeService {
    private readonly IPaymentMethodTypeRepository _typeRepository;

    public PaymentMethodTypeService(IPaymentMethodTypeRepository typeRepository) {
        _typeRepository = typeRepository;
    }

    public async Task<PaymentMethodTypeResponse> GetAllPaymentMethodTypesAsync() {
        var response = new PaymentMethodTypeResponse();
        try {
            var entities = await _typeRepository.GetAllPaymentMethodTypesAsync();
            response.PaymentMethodTypes = entities.Select(MapToDto).ToList();
            response.Success = true;
        } catch (Exception) {
            response.Success = false;
            response.Message = "Failed to fetch payment method types.";
            response.ErrorCode = ExceptionCodes.DbAddError; // Use proper code (here, could be fetch/fallback to DbAddError)
        }
        return response;
    }

    public async Task<PaymentMethodTypeResponse> GetPaymentMethodTypesByCountryAsync(string countryCode) {
        var response = new PaymentMethodTypeResponse();
        try {
            var entities = await _typeRepository.GetPaymentMethodTypesByCountryAsync(countryCode);
            response.PaymentMethodTypes = entities.Select(MapToDto).ToList();
            response.Success = true;
        } catch (Exception) {
            response.Success = false;
            response.Message = "Failed to fetch by country.";
            response.ErrorCode = ExceptionCodes.DbAddError;
        }
        return response;
    }

    public async Task<PaymentMethodTypeResponse?> GetPaymentMethodTypeByIdAsync(Guid id) {
        var response = new PaymentMethodTypeResponse();
        try {
            var entity = await _typeRepository.GetPaymentMethodTypeByIdAsync(id);
            if (entity != null) {
                response.PaymentMethodTypes = new List<PaymentMethodTypeRequest> { MapToDto(entity) };
                response.Success = true;
            }
            else {
                response.Success = false;
                response.Message = "Not found.";
                response.ErrorCode = ExceptionCodes.UserNotFound; // Use something like PaymentAccountErrorCodes.NotFound if you want
            }
        } catch (Exception) {
            response.Success = false;
            response.Message = "Failed to fetch by id.";
            response.ErrorCode = ExceptionCodes.DbAddError;
        }
        return response;
    }

    public async Task<PaymentMethodTypeResponse> AddPaymentMethodTypeAsync(PaymentMethodTypeRequest dto) {
        var response = new PaymentMethodTypeResponse();
        try {
            var entity = MapToEntity(dto);
            var added = await _typeRepository.AddPaymentMethodTypeAsync(entity);
            response.PaymentMethodTypes = new List<PaymentMethodTypeRequest> { MapToDto(added) };
            response.Success = true;
        } catch (Exception) {
            response.Success = false;
            response.Message = "Failed to add.";
            response.ErrorCode = ExceptionCodes.DbAddError;
        }
        return response;
    }

    public async Task<PaymentMethodTypeResponse> UpdatePaymentMethodTypeAsync(PaymentMethodTypeRequest dto) {
        var response = new PaymentMethodTypeResponse();
        try {
            var entity = MapToEntity(dto);
            await _typeRepository.UpdatePaymentMethodTypeAsync(entity);
            response.Success = true;
            response.Message = "Updated successfully.";
        } catch (Exception) {
            response.Success = false;
            response.Message = "Failed to update.";
            response.ErrorCode = ExceptionCodes.DbUpdateError;
        }
        return response;
    }

    public async Task<PaymentMethodTypeResponse> DeletePaymentMethodTypeAsync(Guid id) {
        var response = new PaymentMethodTypeResponse();
        try {
            await _typeRepository.DeletePaymentMethodTypeAsync(id);
            response.Success = true;
            response.Message = "Deleted successfully.";
        } catch (Exception) {
            response.Success = false;
            response.Message = "Failed to delete.";
            response.ErrorCode = ExceptionCodes.DbDeleteError;
        }
        return response;
    }

    // ---- Mapping helpers ----

    private static PaymentMethodTypeRequest MapToDto(PaymentMethodType entity) => new PaymentMethodTypeRequest {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        CountryCode = entity.CountryCode,
        IsForPayment = entity.IsForPayment,
        IsForPayout = entity.IsForPayout,
        IconUrl = entity.IconUrl,
        IsActive = entity.IsActive
    };

    private static PaymentMethodType MapToEntity(PaymentMethodTypeRequest dto) => new PaymentMethodType {
        Id = dto.Id,
        Name = dto.Name ?? "",
        Description = dto.Description,
        CountryCode = dto.CountryCode,
        IsForPayment = dto.IsForPayment,
        IsForPayout = dto.IsForPayout,
        IconUrl = dto.IconUrl,
        IsActive = dto.IsActive // pass through from DTO (lets you deactivate)
    };
}
