using System;

namespace Truckero.Diagnostics.Mocks.Services
{
    // Placeholder entity classes for FreightBidSimulator
    // These can be removed once actual entities are defined in the project

    /// <summary>
    /// Placeholder for FreightRequest entity
    /// </summary>
    public class FreightRequest
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Placeholder for DriverBid entity
    /// </summary>
    public class DriverBid
    {
        public Guid Id { get; set; }
        public Guid FreightRequestId { get; set; }
        public Guid DriverProfileId { get; set; }
        public decimal Price { get; set; }
        public DateTime EstimatedArrivalTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Placeholder for DriverProfile entity
    /// </summary>
    public class DriverProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string LicenseFrontUrl { get; set; } = string.Empty;
        public string LicenseBackUrl { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public User? User { get; set; }
    }

    /// <summary>
    /// Placeholder for User entity
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}