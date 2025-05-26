using Truckero.Core.DTOs.Auth;
using Truckero.Core.Entities;
using Truckero.Core.Enums;
using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Services;

namespace TruckeroApp.ServiceClients.Mock;

public class AuthMockService : IAuthService
{
    private string _activeRole = RoleType.Driver.ToString(); // default
    private readonly List<string> _mockRoles = new() { "Customer", "Driver", "StoreClerk" };

    private static readonly Guid _mockUserId = Guid.Parse("18b1b874-bab5-449d-8ff0-251758e9621b");
    private const string _mockEmail = "mockuser@truckero.app";
    private const string _mockAccessToken =
        "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9." +
        "eyJzdWIiOiIxMjM0NTY3ODkwIiwiZW1haWwiOiJtb2NrdXNlckB0cnVja2Vyby5hcHAiLCJ1c2VySWQiOiIxOGIxYjg3NC1iYWI1LTQ0OWQtOGZmMC0yNTE3NThlOTYyMWIiLCJleHAiOjIxOTU3NTI4MDB9." +
        "FAKE_SIGNATURE";
    private const string _mockRefreshToken = "mock-refresh-token";

    private readonly IAuthTokenRepository _tokenRepo;

    public AuthMockService(IAuthTokenRepository tokenRepo)
    {
        _tokenRepo = tokenRepo;
    }

    public async Task<AuthResponse> LoginUserAsync(AuthLoginRequest request)
    {
        if (request.Email == "invalid@example.com" && request.Password == "wrongpass")
            throw new UnauthorizedAccessException("Invalid credentials");

        var token = new AuthToken
        {
            UserId = _mockUserId,
            AccessToken = _mockAccessToken,
            RefreshToken = _mockRefreshToken,
            Role = _activeRole,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        await _tokenRepo.AddTokenAsync(token);

        return new AuthResponse
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken
        };
    }

    public Task<AuthResponse> RegisterUserAsync(RegisterUserRequest request)
    {
        return LoginUserAsync(new AuthLoginRequest { Email = request.Email, Password = "password" });
    }

    public async Task LogoutUserAsync(Guid userId)
    {
        var token = await _tokenRepo.GetByTokenByUserIdAsync(userId);
        if (token != null)
            await _tokenRepo.DeleteTokenAsync(token);
    }

    public Task<AuthResponse> ExchangeTokenAsync(TokenRequest request)
    {
        return Task.FromResult(new AuthResponse
        {
            AccessToken = "exchanged.jwt.token",
            RefreshToken = "exchanged.refresh.token"
        });
    }

    public async Task<AuthResponse> RefreshAccessTokenAsync(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new ArgumentException("Missing refresh token");

        if (request.RefreshToken == "throw-exception-token")
            throw new InvalidOperationException("Mocked backend exception");

        var existing = await _tokenRepo.GetByRefreshTokenByRefreshTokenKeyAsync(request.RefreshToken);
        if (existing == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        var refreshed = new AuthToken
        {
            UserId = existing.UserId,
            AccessToken = "refreshed.jwt.token",
            RefreshToken = "refreshed.refresh.token",
            Role = existing.Role,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        await _tokenRepo.UpdateTokenAsync(refreshed);

        return new AuthResponse
        {
            AccessToken = refreshed.AccessToken,
            RefreshToken = refreshed.RefreshToken
        };
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        // Simulate expired/invalid tokens more reliably
        return Task.FromResult(
            !string.IsNullOrWhiteSpace(token)
            && token.StartsWith("ey")
            && !token.StartsWith("eyFake") // reject mock expired token
        );
    }


    public Task RequestPasswordResetAsync(PasswordResetRequest request)
    {
        return Task.CompletedTask;
    }

    public Task ConfirmPasswordResetAsync(PasswordResetConfirmRequest request)
    {
        return Task.CompletedTask;
    }

    public Task<List<string>> GetAllRolesAsync()
    {
        return Task.FromResult(_mockRoles);
    }

    public async Task<string> GetActiveRoleAsync()
    {
        var latest = await _tokenRepo.GetLatestTokenAsync();
        return latest?.Role ?? "Unknown";
    }

    public async Task SetActiveRoleAsync(string role)
    {
        _activeRole = role;

        var latest = await _tokenRepo.GetLatestTokenAsync();
        if (latest != null)
        {
            latest.Role = role;
            await _tokenRepo.UpdateTokenAsync(latest);
        }
    }

    public async Task<SessionInfo> GetSessionAsync()
    {
        var token = await _tokenRepo.GetLatestTokenAsync();

        return new SessionInfo
        {
            UserId = _mockUserId,
            Email = _mockEmail,
            FullName = "Mock User",
            ActiveRole = token?.Role ?? _activeRole,
            AvailableRoles = _mockRoles,
            TokenValid = token != null
        };
    }

    public Task<AuthToken?> GetLatestAsync()
    {
        return _tokenRepo.GetLatestTokenAsync();
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        // Return a mock user if the email matches the mock email, otherwise null
        if (email == _mockEmail)
        {
            return Task.FromResult<User?>(new User
            {
                Id = _mockUserId,
                Email = _mockEmail
            });
        }
        return Task.FromResult<User?>(null);
    }

    public Task<User?> GetUserByIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    Task<(User NewUser, AuthToken Token)> IAuthService.RegisterUserAsync(RegisterUserRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetCurrentUserAsync()
    {
        // Return a mock user with EmailVerified = false for testing
        return Task.FromResult<User?>(new User
        {
            Id = _mockUserId,
            Email = _mockEmail,
            EmailVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    }
}
