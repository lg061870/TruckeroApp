using Truckero.Core.DTOs.Shared;

namespace Truckero.Core.Interfaces.Repositories; 
public interface IBankRepository {
    // Create
    Task<BankRequest> AddBankAsync(BankRequest bank);

    // Read (all for country)
    Task<List<BankRequest>> GetBanksByCountryCodeAsync(string countryCode);

    // Read (by ID)
    Task<BankRequest?> GetBankByIdAsync(string bankId);

    // Update
    Task UpdateBankAsync(BankRequest bank);

    // Inactivate (soft delete/disable)
    Task InactivateBankAsync(string bankId);

    // Delete (hard delete, optional)
    Task DeleteBankAsync(string bankId);
}
