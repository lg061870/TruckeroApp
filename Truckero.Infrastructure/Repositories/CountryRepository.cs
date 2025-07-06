using Microsoft.EntityFrameworkCore;
using Truckero.Core.DTOs.Shared;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories;

public class CountryRepository : ICountryRepository {
    private readonly AppDbContext _context;

    public CountryRepository(AppDbContext context) {
        _context = context;
    }

    public async Task<CountryRequest> AddCountryAsync(CountryRequest country) {
        var entity = new Country {
            Code = country.Code,
            Name = country.Name,
            IsActive = true
        };
        _context.Countries.Add(entity);
        await _context.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<List<CountryRequest>> GetAllCountriesAsync() {
        return await _context.Countries
            .Where(c => c.IsActive)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<CountryRequest?> GetByCodeAsync(string countryCode) {
        var entity = await _context.Countries.FindAsync(countryCode);
        return entity is { IsActive: true } ? MapToDto(entity) : null;
    }

    public async Task UpdateCountryAsync(CountryRequest country) {
        var entity = await _context.Countries.FindAsync(country.Code);
        if (entity == null || !entity.IsActive) throw new KeyNotFoundException("Country not found.");
        entity.Name = country.Name;
        // Update other fields as needed
        await _context.SaveChangesAsync();
    }

    public async Task InactivateCountryAsync(string countryCode) {
        var entity = await _context.Countries.FindAsync(countryCode);
        if (entity == null) throw new KeyNotFoundException("Country not found.");
        entity.IsActive = false;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCountryAsync(string countryCode) {
        var entity = await _context.Countries.FindAsync(countryCode);
        if (entity == null) throw new KeyNotFoundException("Country not found.");
        _context.Countries.Remove(entity);
        await _context.SaveChangesAsync();
    }

    private static CountryRequest MapToDto(Country entity) =>
        new CountryRequest {
            Code = entity.Code,
            Name = entity.Name
            // Map more fields as needed
        };
}