using Truckero.Core.DTOs.Shared;

namespace Truckero.Core.Interfaces.Services;

public interface ICountryService {
    Task<List<CountryRequest>> GetAllCountriesAsync();
    Task<CountryRequest?> GetByCodeAsync(string countryCode);
    // Add more as needed (e.g., Add, Update, etc.)
}
