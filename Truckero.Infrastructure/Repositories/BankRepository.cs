using Truckero.Core.DTOs.Shared;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories; 

public class BankRepository : IBankRepository {
    private readonly AppDbContext _context;

    public BankRepository(AppDbContext context) {
        _context = context;
    }

    public async Task<BankRequest> AddBankAsync(BankRequest bank) {
        var entity = new Bank {
            Id = Guid.NewGuid(),
            Name = bank.Name,
            CountryCode = bank.CountryCode,
            // Add other fields as needed
            IsActive = true
        };
        _context.Banks.Add(entity);
        await _context.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<List<BankRequest>> GetBanksByCountryCodeAsync(string countryCode) {
        return _context.Banks
            .Where(b => b.CountryCode == countryCode && b.IsActive)
            .Select(b => MapToDto(b))
            .ToList();
    }

    public async Task<BankRequest?> GetBankByIdAsync(string bankId) {
        var entity = await _context.Banks.FindAsync(bankId);
        return entity is { IsActive: true } ? MapToDto(entity) : null;
    }

    public async Task UpdateBankAsync(BankRequest bank) {
        var entity = await _context.Banks.FindAsync(bank.Id);
        if (entity == null || !entity.IsActive) throw new KeyNotFoundException("Bank not found.");
        entity.Name = bank.Name;
        entity.CountryCode = bank.CountryCode;
        // Update other fields as needed
        await _context.SaveChangesAsync();
    }

    public async Task InactivateBankAsync(string bankId) {
        var entity = await _context.Banks.FindAsync(bankId);
        if (entity == null) throw new KeyNotFoundException("Bank not found.");
        entity.IsActive = false;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteBankAsync(string bankId) {
        var entity = await _context.Banks.FindAsync(bankId);
        if (entity == null) throw new KeyNotFoundException("Bank not found.");
        _context.Banks.Remove(entity);
        await _context.SaveChangesAsync();
    }

    // Helper for mapping entity to DTO
    private static BankRequest MapToDto(Bank entity) =>
        new BankRequest {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            CountryCode = entity.CountryCode
            // Map more fields as needed
        };
}
