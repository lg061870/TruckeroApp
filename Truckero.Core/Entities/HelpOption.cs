// File: Truckero.Core.Entities/HelpOption.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Truckero.Core.Entities {
    public class HelpOption {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
    }
}
