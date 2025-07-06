using Truckero.Core.DTOs.Shared;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;

namespace Truckero.Infrastructure.Services;

public class BankService : IBankService {
    private readonly IBankRepository _bankRepository;

    public BankService(IBankRepository bankRepository) {
        _bankRepository = bankRepository;
    }

    public async Task<List<BankRequest>> GetBanksByCountryCodeAsync(string countryCode)
        => await _bankRepository.GetBanksByCountryCodeAsync(countryCode);

    public async Task<BankRequest?> GetBankByIdAsync(string bankId)
        => await _bankRepository.GetBankByIdAsync(bankId);

    // Add more methods as needed
}
