using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;
public interface IHelpOptionRepository {
    Task<List<HelpOption>> GetAllAsync();
    Task<HelpOption?> GetByIdAsync(Guid id);
    // Add more methods as needed (e.g., AddAsync, DeleteAsync, etc.)
}
