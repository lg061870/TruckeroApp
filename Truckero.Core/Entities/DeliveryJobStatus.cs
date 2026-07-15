namespace Truckero.Core.Entities;

public enum DeliveryJobStatus {
    Pending = 0,
    Assigned = 1,
    InTransit = 2,
    Delivered = 3,
    Cancelled = 4,
    Failed = 5
}
