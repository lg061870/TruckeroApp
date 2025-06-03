namespace Truckero.Core.Entities;

public class MediaAsset
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string Url { get; set; } = null!;           // If stored externally (e.g., Azure Blob)
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Guid? UserId { get; set; }
    public Guid? VehicleId { get; set; }
    public string? Purpose { get; set; }               // e.g., "DriverLicenseFront", "InsuranceDoc"

    // Navigation (optional)
    public User? User { get; set; }
    public Truck? Vehicle { get; set; }
}