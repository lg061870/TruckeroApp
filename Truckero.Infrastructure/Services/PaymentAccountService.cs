using Truckero.Core.DTOs.PaymentAccount;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;

namespace Truckero.Infrastructure.Services; 
public class PaymentAccountService : IPaymentAccountService {
    private readonly IPaymentAccountRepository _accountRepo;

    public PaymentAccountService(IPaymentAccountRepository accountRepo) {
        _accountRepo = accountRepo;
    }

    public async Task<PaymentAccountResponse> GetPaymentAccountsByUserIdAsync(Guid userId) {
        var accounts = await _accountRepo.GetPaymentAccountsByUserIdAsync(userId);
        return new PaymentAccountResponse {
            Success = true,
            PaymentAccounts = accounts.Select(MapEntityToDto).ToList(),
            Message = accounts.Any() ? null : "No payment accounts found."
        };
    }

    public async Task<PaymentAccountResponse> GetPaymentAccountByIdAsync(Guid paymentAccountId) {
        var entity = await _accountRepo.GetPaymentAccountByIdAsync(paymentAccountId);
        if (entity == null)
            return new PaymentAccountResponse { Success = false, Message = "Not found", ErrorCode = "NOT_FOUND" };

        return new PaymentAccountResponse {
            Success = true,
            PaymentAccounts = new List<PaymentAccountRequest> { MapEntityToDto(entity) }
        };
    }

    public async Task<PaymentAccountResponse> AddPaymentAccountAsync(PaymentAccountRequest request) {
        var entity = MapDtoToEntity(request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;

        await _accountRepo.AddPaymentAccountAsync(entity);
        await _accountRepo.SaveChangesAsync();

        return new PaymentAccountResponse {
            Success = true,
            PaymentAccounts = new List<PaymentAccountRequest> { MapEntityToDto(entity) }
        };
    }

    public async Task<PaymentAccountResponse> UpdatePaymentAccountAsync(PaymentAccountRequest request) {
        var entity = await _accountRepo.GetPaymentAccountByIdAsync(request.Id);
        if (entity == null)
            return new PaymentAccountResponse { Success = false, Message = "Not found", ErrorCode = "NOT_FOUND" };

        // Map updatable fields from request to entity
        entity.PaymentMethodTypeId = request.PaymentMethodTypeId;
        entity.FullName = request.FullName;
        entity.BankId = request.BankId;
        entity.AccountNumberLast4 = request.AccountNumberLast4;
        entity.RoutingNumber = request.RoutingNumber;
        entity.MobileNumber = request.MobileNumber;
        entity.PayPalEmail = request.PayPalEmail;
        entity.IsDefault = request.IsDefault;
        entity.IsValidated = request.IsValidated;
        entity.MetadataJson = request.MetadataJson;
        entity.PaymentAccountNickName = request.PaymentAccountNickName;

        await _accountRepo.UpdatePaymentAccountAsync(entity);
        await _accountRepo.SaveChangesAsync();

        return new PaymentAccountResponse {
            Success = true,
            PaymentAccounts = new List<PaymentAccountRequest> { MapEntityToDto(entity) }
        };
    }

    public async Task<PaymentAccountResponse> DeletePaymentAccountAsync(Guid userId, Guid paymentAccountId) {
        var entity = await _accountRepo.GetPaymentAccountByIdAsync(paymentAccountId);
        if (entity == null || entity.UserId != userId)
            return new PaymentAccountResponse { Success = false, Message = "Not found or unauthorized", ErrorCode = "NOT_FOUND" };

        await _accountRepo.DeletePaymentAccountAsync(paymentAccountId);
        await _accountRepo.SaveChangesAsync();

        return new PaymentAccountResponse {
            Success = true,
            Message = "Deleted"
        };
    }

    public async Task<PaymentAccountResponse> SetDefaultPaymentAccountAsync(Guid userId, Guid paymentAccountId) {
        await _accountRepo.SetDefaultPaymentAccountAsync(userId, paymentAccountId);
        await _accountRepo.SaveChangesAsync();
        return new PaymentAccountResponse { Success = true, Message = "Default set" };
    }

    public async Task<PaymentAccountResponse> MarkPaymentAccountValidatedAsync(Guid userId, Guid paymentAccountId) {
        await _accountRepo.MarkPaymentAccountValidatedAsync(userId, paymentAccountId);
        await _accountRepo.SaveChangesAsync();
        return new PaymentAccountResponse { Success = true, Message = "Account validated" };
    }

    // ===== MAPPING HELPERS =====
    private static PaymentAccountRequest MapEntityToDto(PaymentAccount entity) {
        return new PaymentAccountRequest {
            Id = entity.Id,
            UserId = entity.UserId,
            PaymentMethodTypeId = entity.PaymentMethodTypeId,
            FullName = entity.FullName ?? "",
            BankId = entity.BankId,
            AccountNumberLast4 = entity.AccountNumberLast4,
            RoutingNumber = entity.RoutingNumber,
            MobileNumber = entity.MobileNumber,
            PayPalEmail = entity.PayPalEmail,
            IsDefault = entity.IsDefault,
            IsValidated = entity.IsValidated,
            MetadataJson = entity.MetadataJson,
            PaymentAccountNickName = entity.PaymentAccountNickName
        };
    }
    private static PaymentAccount MapDtoToEntity(PaymentAccountRequest dto) {
        return new PaymentAccount {
            Id = dto.Id,
            UserId = dto.UserId,
            PaymentMethodTypeId = dto.PaymentMethodTypeId,
            FullName = dto.FullName ?? "",
            BankId = dto.BankId,
            AccountNumberLast4 = dto.AccountNumberLast4,
            RoutingNumber = dto.RoutingNumber,
            MobileNumber = dto.MobileNumber,
            PayPalEmail = dto.PayPalEmail,
            IsDefault = dto.IsDefault,
            IsValidated = dto.IsValidated,
            MetadataJson = dto.MetadataJson,
            PaymentAccountNickName = dto.PaymentAccountNickName
        };
    }
}
