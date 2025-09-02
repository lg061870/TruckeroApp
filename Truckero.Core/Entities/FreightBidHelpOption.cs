using System.ComponentModel.DataAnnotations;
using Truckero.Core.Entities;

public class FreightBidHelpOption {
    [Key]
    public Guid Id { get; set; }
    public Guid FreightBidId { get; set; }
    public Guid HelpOptionId { get; set; }

    // Navigation
    public FreightBid FreightBid { get; set; }
    public HelpOption HelpOption { get; set; }
}
