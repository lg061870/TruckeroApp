namespace Truckero.Store.Models;

public sealed class StoreClerkAccountDraft
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
