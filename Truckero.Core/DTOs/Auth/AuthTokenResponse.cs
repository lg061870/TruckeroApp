namespace Truckero.Core.DTOs.Auth;

public class AuthTokenResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public object ExpiresIn { get; set; }
}