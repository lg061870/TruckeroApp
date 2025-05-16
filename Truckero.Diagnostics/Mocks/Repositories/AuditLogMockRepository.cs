using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;

namespace Truckero.Diagnostics.Mocks;

/// <summary>
/// In-memory fake audit logger for unit testing.
/// Captures all logs and supports simple querying.
/// </summary>
public class AuditLogMockRepository : IAuditLogRepository
{
    private readonly List<AuditLog> _logs = new();

    public Task LogAsync(AuditLog entry)
    {
        _logs.Add(entry);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<AuditLog>> GetLogsByUserAsync(Guid userId)
    {
        var result = _logs.Where(log => log.UserId == userId);
        return Task.FromResult(result);
    }

    public Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 50)
    {
        var result = _logs
            .OrderByDescending(log => log.Timestamp)
            .Take(count);
        return Task.FromResult(result);
    }

    // ✅ Optional helper for tests
    public AuditLog? FindByAction(string action)
    {
        return _logs.FirstOrDefault(log => log.Action == action);
    }

    // ✅ Optional: expose all logs for assertions
    public IReadOnlyList<AuditLog> All => _logs.AsReadOnly();
}
