using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Truckero.Core.Entities;

public class AuditLog
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Action { get; set; } = null!;
    public string TargetType { get; set; } = null!;
    public Guid? TargetId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
    public string Description { get; set; }
}
