using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Services;
using Truckero.Core.Interfaces.Repositories;

namespace Truckero.Infrastructure.Services; 

public class HelpOptionService : IHelpOptionService {
    private readonly IHelpOptionRepository _repository;
    public HelpOptionService(IHelpOptionRepository repository) {
        _repository = repository;
    }

    public async Task<List<HelpOption>> GetAllHelpOptionsAsync() {
        return await _repository.GetAllAsync();
    }

    public async Task<HelpOption?> GetHelpOptionByIdAsync(Guid id) {
        return await _repository.GetByIdAsync(id);
    }
    // Add more methods as needed
}
