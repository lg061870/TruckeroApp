using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Truckero.Core.Entities;

public class FreightBidUseTag {
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid FreightBidId { get; set; }
    [ForeignKey(nameof(FreightBidId))]
    public FreightBid FreightBid { get; set; } = null!;

    [Required]
    public Guid UseTagId { get; set; }
    [ForeignKey(nameof(UseTagId))]
    public UseTag UseTag { get; set; } = null!;
}
