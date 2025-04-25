namespace Truckero.Core.Entities;

public class AuditLog
{
   public Guid Id { get; set; }
    public Guid UserId { get; set; }
 

   public string Action { get; set; } = null!;
    public string TargetType { get; set; } = null!;
 

   public Guid? TargetId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
 

   public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }



    public User User { get; set; } = null!;
}


