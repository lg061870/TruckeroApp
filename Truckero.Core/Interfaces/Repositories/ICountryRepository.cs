using Truckero.Core.DTOs.Shared;

namespace Truckero.Core.Interfaces.Repositories;

public interface ICountryRepository {
    // Create
    Task<CountryRequest> AddCountryAsync(CountryRequest country);

    // Read (all)
    Task<List<CountryRequest>> GetAllCountriesAsync();

    // Read (by code)
    Task<CountryRequest?> GetByCodeAsync(string countryCode);

    // Update
    Task UpdateCountryAsync(CountryRequest country);

    // Inactivate (soft delete/disable)
    Task InactivateCountryAsync(string countryCode);

    // Delete (hard delete, optional)
    Task DeleteCountryAsync(string countryCode);
}