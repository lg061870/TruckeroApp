using Truckero.Core.DTOs.Shared;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;

namespace Truckero.Infrastructure.Services;

public class CountryService : ICountryService {
    private readonly ICountryRepository _countryRepository;

    public CountryService(ICountryRepository countryRepository) {
        _countryRepository = countryRepository;
    }

    public async Task<List<CountryRequest>> GetAllCountriesAsync()
        => await _countryRepository.GetAllCountriesAsync();

    public async Task<CountryRequest?> GetByCodeAsync(string countryCode)
        => await _countryRepository.GetByCodeAsync(countryCode);

    // Add more methods as needed (e.g., AddCountryAsync, UpdateCountryAsync)
}
