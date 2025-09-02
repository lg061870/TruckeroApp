namespace Truckero.Core.DTOs.CustomerFlow; 
public class DriverBidRequest {
    public Guid DriverBidId { get; set; }
    public Guid FreightBidId { get; set; }
    public Guid DriverId { get; set; }
    public Guid TruckId { get; set; }
    public decimal OfferAmount { get; set; }
    public string? Message { get; set; }
}