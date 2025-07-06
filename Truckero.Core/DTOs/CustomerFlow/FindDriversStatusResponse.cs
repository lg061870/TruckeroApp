namespace Truckero.Core.DTOs.CustomerFlow;

public class FindDriversStatusResponse {
    public Guid FreightBidId { get; set; }
    public bool DriversFound { get; set; }
    public int TotalDriversFound { get; set; }
    public DateTime RequestTime { get; set; }
    public string? StatusMessage { get; set; }
}