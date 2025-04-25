using Truckero.Core.DTOs.Auth;

namespace Truckero.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterUserRequest request);
    Task<AuthResponse> LoginAsync(AuthLoginRequest request);
    Task LogoutAsync(Guid userId);
}

