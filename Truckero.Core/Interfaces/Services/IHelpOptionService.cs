using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Services; 

public interface IHelpOptionService {
    Task<List<HelpOption>> GetAllHelpOptionsAsync();
    Task<HelpOption?> GetHelpOptionByIdAsync(Guid id);
    // Add more methods as needed
}
