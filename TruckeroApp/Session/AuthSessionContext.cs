namespace TruckeroApp.Session;

public class AuthSessionContext
{
    public Guid? UserId { get; set; }
    public string? AccessToken { get; set; }

    public string? ActiveRole { get; set; }
    public List<string> AvailableRoles { get; set; } = new();

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);

    public void Clear()
    {
        UserId = null;
        AccessToken = null;
        ActiveRole = null;
        AvailableRoles.Clear();
    }
}
