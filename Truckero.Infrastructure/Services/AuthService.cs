using Truckero.Core.DTOs.Auth;
using Truckero.Core.Enums;
using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;
using Truckero.Core.Entities;
using Microsoft.Extensions.Logging;

namespace Truckero.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IAuthTokenRepository _tokenRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IHashService _hashService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepo,
        IAuthTokenRepository tokenRepo,
        IRoleRepository roleRepo,
        IHashService hashService,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _userRepo = userRepo;
        _tokenRepo = tokenRepo;
        _roleRepo = roleRepo;
        _hashService = hashService;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<AuthResponse> LoginAsync(AuthLoginRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);
        if (user == null || !_hashService.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid login attempt for email: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var token = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccessToken = $"token-{Guid.NewGuid()}",
            RefreshToken = $"refresh-{Guid.NewGuid()}",
            Role = user.Role?.Name.ToString() ?? RoleType.Guest.ToString(),
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        await _tokenRepo.AddAsync(token);

        return new AuthResponse
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken
        };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterUserRequest request)
    {
        var existingUser = await _userRepo.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("User already exists");

        var defaultRoleId = await _roleRepo.GetDefaultRoleIdAsync();

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = _hashService.Hash(request.Password),
            RoleId = defaultRoleId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmailVerified = false
        };

        await _userRepo.AddAsync(newUser);
        await _userRepo.SaveChangesAsync();

        return await LoginAsync(new AuthLoginRequest { Email = request.Email, Password = request.Password });
    }

    public async Task LogoutAsync(Guid userId)
    {
        var token = await _tokenRepo.GetByUserIdAsync(userId);
        if (token != null)
        {
            await _tokenRepo.DeleteAsync(token);
            _logger.LogInformation("User {UserId} logged out.", userId);
        }
    }



    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new ArgumentException("Refresh token is missing.");

        var existing = await _tokenRepo.GetByRefreshTokenAsync(request.RefreshToken);
        if (existing == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        var newToken = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = existing.UserId,
            AccessToken = $"token-{Guid.NewGuid()}",
            RefreshToken = $"refresh-{Guid.NewGuid()}",
            Role = existing.Role,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        await _tokenRepo.UpdateAsync(newToken);

        return new AuthResponse
        {
            AccessToken = newToken.AccessToken,
            RefreshToken = newToken.RefreshToken
        };
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        var isValid = !string.IsNullOrWhiteSpace(token) && token.StartsWith("token-");
        return Task.FromResult(isValid);
    }

    public async Task RequestPasswordResetAsync(PasswordResetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required");

        var user = await _userRepo.GetByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Password reset requested for unknown email: {Email}", request.Email);
            throw new KeyNotFoundException("No user found with that email");
        }

        // 🚧 TODO: Replace with secure token gen
        var resetToken = "some-token";

        var resetLink = $"https://app.truckero.com/confirm-reset?email={Uri.EscapeDataString(request.Email)}&token={resetToken}";

        await _emailService.SendPasswordResetAsync(request.Email, resetLink);

        _logger.LogInformation("Password reset email sent to {Email} with link: {Link}", request.Email, resetLink);
    }



    public async Task ConfirmPasswordResetAsync(PasswordResetConfirmRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);
        if (user == null)
            throw new ArgumentException("User not found");

        user.PasswordHash = _hashService.Hash(request.NewPassword);
        await _userRepo.SaveChangesAsync();
    }

    public Task<AuthToken?> GetLatestAsync() =>
        _tokenRepo.GetLatestAsync();

    public async Task<List<string>> GetAllRolesAsync()
    {
        var roles = await _roleRepo.GetAllRolesAsync();
        return roles.Select(r => r.Name.ToString()).ToList();
    }

    public async Task SetActiveRoleAsync(string role)
    {
        var token = await _tokenRepo.GetLatestAsync();
        if (token == null)
            throw new InvalidOperationException("No active session");

        token.Role = role;
        await _tokenRepo.UpdateAsync(token);
    }

    public async Task<SessionInfo> GetSessionAsync()
    {
        var token = await _tokenRepo.GetLatestAsync();

        return new SessionInfo
        {
            UserId = token?.UserId ?? Guid.Empty,
            Email = "user@truckero.app", // Substitute if needed
            FullName = "Truckero User",
            ActiveRole = token?.Role ?? RoleType.Guest.ToString(),
            AvailableRoles = await GetAllRolesAsync(),
            TokenValid = token != null
        };
    }

    public async Task<string> GetActiveRoleAsync()
    {
        var token = await _tokenRepo.GetLatestAsync();
        return token?.Role ?? RoleType.Guest.ToString();
    }

    public async Task<AuthResponse> ExchangeTokenAsync(TokenRequest request)
    {
        // 🔍 Look up the user by email
        var user = await _userRepo.GetByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        // 🔐 Validate password
        if (!_hashService.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid password.");

        // ♻️ Issue a new token for the user
        var newToken = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccessToken = $"token-{Guid.NewGuid()}",
            RefreshToken = $"refresh-{Guid.NewGuid()}",
            Role = user.Role?.Name ?? RoleType.Guest.ToString(),
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        await _tokenRepo.UpdateAsync(newToken);

        return new AuthResponse
        {
            AccessToken = newToken.AccessToken,
            RefreshToken = newToken.RefreshToken
        };
    }


}
