using Truckero.Core.Entities;

namespace Truckero.Core.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task LogAsync(AuditLog entry);
    Task<IEnumerable<AuditLog>> GetLogsByUserAsync(Guid userId);
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 50);
}
