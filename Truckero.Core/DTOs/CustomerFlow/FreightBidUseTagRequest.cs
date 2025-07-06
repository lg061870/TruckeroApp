using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.DTOs.CustomerFlow; 
public class FreightBidUseTagRequest {
    [Required]
    public Guid FreightBidId { get; set; }

    [Required]
    public Guid UseTagId { get; set; }
}