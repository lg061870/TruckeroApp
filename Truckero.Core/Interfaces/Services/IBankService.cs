using Truckero.Core.DTOs.PaymentMethodType;
using Truckero.Core.DTOs.Shared;

namespace Truckero.Core.Interfaces.Services;

public interface IBankService {
    Task<List<BankRequest>> GetBanksByCountryCodeAsync(string countryCode);
    Task<BankRequest?> GetBankByIdAsync(string bankId);
    // Add more as needed
}