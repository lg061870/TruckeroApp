using Truckero.Core.DTOs.Auth;
using Truckero.Core.Entities;
using Truckero.Core.Enums;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Truckero.Core.Constants;

namespace Truckero.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IAuthTokenRepository _tokenRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IHashService _hashService;
    private readonly IEmailService _emailService;
    private readonly ICustomerRepository _customerRepo;
    private readonly IConfirmationTokenRepository _confirmationTokenRepo;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepo,
        IAuthTokenRepository tokenRepo,
        IRoleRepository roleRepo,
        IHashService hashService,
        IEmailService emailService,
        ICustomerRepository customerRepo,
        IConfirmationTokenRepository confirmationTokenRepo,
        ILogger<AuthService> logger)
    {
        _userRepo = userRepo;
        _tokenRepo = tokenRepo;
        _roleRepo = roleRepo;
        _hashService = hashService;
        _emailService = emailService;
        _customerRepo = customerRepo;
        _confirmationTokenRepo = confirmationTokenRepo;
        _logger = logger;
    }

    #region 🔐 Core Authentication Lifecycle

    // --- RegisterUserAsync ---
    public async Task<(User NewUser, AuthToken Token)> RegisterUserAsync(RegisterUserRequest request)
    {
        var existingUser = await _userRepo.GetUserByEmailAsync(request.Email);
        if (existingUser != null)
            throw new OnboardingStepException("User already exists", "email_already_exist");


        // Use the role from the request, not the default
        var roleEntity = await _roleRepo.GetByNameAsync(request.Role);
        if (roleEntity == null)
            throw new OnboardingStepException($"Role '{request.Role}' does not exist.", "role_not_found");
        var roleId = roleEntity.Id;

        var userId = request.UserId == Guid.Empty
            ? request.UserId
            : Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = request.Email,
            PasswordHash = _hashService.Hash(request.Password),
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmailVerified = false,
            PhoneNumber = request.PhoneNumber
        };

        var token = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccessToken = $"token-{Guid.NewGuid()}",
            RefreshToken = $"refresh-{Guid.NewGuid()}",
            Role = request.Role,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _userRepo.AddUserAsync(user);
        await _tokenRepo.AddTokenAsync(token);

        return (user, token);
    }

    public async Task<AuthResponse> LoginUserAsync(AuthLoginRequest request)
    {
        var user = await _userRepo.GetUserByEmailAsync(request.Email);

        if (user == null)
        {
            _logger.LogWarning("Login failed: user not found for email {Email}", request.Email);
            throw new LoginStepException("Account not available.", ExceptionCodes.AccountNotAvailable);
        }

        if (!user.EmailVerified)
        {
            throw new LoginStepException("Email not verified. Please check your inbox for a confirmation email.", ExceptionCodes.EmailNotVerified);
        }

        if (!_hashService.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: invalid password for email {Email}", request.Email);
            throw new LoginStepException("Login failed: invalid credentials.", ExceptionCodes.LoginFailure);
        }

        // 🔄 Revoke stale tokens in one go for this User
        await _tokenRepo.RevokeTokensByUserIdAsync(user.Id);

        // Check for valid token
        var existingToken = await _tokenRepo.GetTokenByUserIdAsync(user.Id);

        if (existingToken != null)
        {
            return new AuthResponse
            {
                AccessToken = existingToken.AccessToken,
                RefreshToken = existingToken.RefreshToken,
                Success = true,
                UserId = user.Id
            };
        }

        // 🔐 Create new token
        var newToken = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccessToken = $"token-{Guid.NewGuid()}",
            RefreshToken = $"refresh-{Guid.NewGuid()}",
            Role = user.Role?.Name?.ToString() ?? RoleType.Guest.ToString(),
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _tokenRepo.AddTokenAsync(newToken);

        return new AuthResponse
        {
            AccessToken = newToken.AccessToken,
            RefreshToken = newToken.RefreshToken,
            Success = true,
            UserId = user.Id,
            Role = user.Role?.Name?.ToString()
        };
    }



    public async Task LogoutUserAsync(Guid userId)
    {
        var token = await _tokenRepo.GetTokenByUserIdAsync(userId);
        if (token != null)
        {
            await _tokenRepo.DeleteTokenAsync(token);
            _logger.LogInformation("User {UserId} logged out.", userId);
        }
    }

    public async Task<AuthResponse> ExchangeTokenAsync(TokenRequest request)
    {
        var user = await _userRepo.GetUserByEmailAsync(request.Email);
        if (user == null || !_hashService.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccessToken = $"token-{Guid.NewGuid()}",
            RefreshToken = $"refresh-{Guid.NewGuid()}",
            Role = user.Role?.Name ?? RoleType.Guest.ToString(),
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _tokenRepo.UpdateTokenAsync(token);

        return new AuthResponse
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken
        };
    }

    public async Task<AuthResponse> RefreshAccessTokenAsync(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new ArgumentException("Refresh token is missing.");

        var existing = await _tokenRepo.GetByRefreshTokenByRefreshTokenKeyAsync(request.RefreshToken);
        if (existing == null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        existing.AccessToken = $"token-{Guid.NewGuid()}";
        existing.RefreshToken = $"refresh-{Guid.NewGuid()}";
        existing.IssuedAt = DateTime.UtcNow;
        existing.ExpiresAt = DateTime.UtcNow.AddDays(7);

        await _tokenRepo.UpdateTokenAsync(existing);

        return new AuthResponse
        {
            AccessToken = existing.AccessToken,
            RefreshToken = existing.RefreshToken,
            UserId = existing.UserId
        };
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        var isValid = !string.IsNullOrWhiteSpace(token) && token.StartsWith("token-");
        return Task.FromResult(isValid);
    }

    #endregion

    #region 🔑 Password Recovery

    public async Task RequestPasswordResetAsync(PasswordResetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required");

        var user = await _userRepo.GetUserByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Password reset requested for unknown email: {Email}", request.Email);
            // Optional: throw to preserve current behavior OR return silently to avoid enumeration
            throw new KeyNotFoundException("No user found with that email");
        }

        var token = new ConfirmationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Guid.NewGuid().ToString(),
            Used = false,
            CreatedAt = DateTime.UtcNow,
            Type = ConfirmationTokenType.PasswordReset
        };

        await _confirmationTokenRepo.AddConfirmationTokenAsync(token);
        await _confirmationTokenRepo.SaveConfirmationTokenChangesAsync();

        var resetLink = $"https://localhost:5001/reset-password?token={token.Token}"; // or use config-based base URL
        await _emailService.SendPasswordResetAsync(user.Email, resetLink);

        _logger.LogInformation("Password reset email sent to {Email}", user.Email);
    }


    public async Task ConfirmPasswordResetAsync(PasswordResetConfirmRequest request)
    {
        // check the request is not null
        if (request == null)
            throw new ArgumentNullException(nameof(request), "Password Reset object is null");

        if (string.IsNullOrWhiteSpace(request?.Email))
            throw new ArgumentException("Email is required");

        if (string.IsNullOrWhiteSpace(request?.NewPassword))
            throw new ArgumentException("New password is required");

        var user = await _userRepo.GetUserByEmailAsync(request.Email);

        if (user == null)
            throw new ArgumentException("User not found");

        user.PasswordHash = _hashService.Hash(request.NewPassword);

        await _userRepo.SaveUserChangesAsync();
    }

    #endregion

    #region 🧭 Role-Based Access Control

    public async Task<List<string>> GetAllRolesAsync()
    {
        var roles = await _roleRepo.GetAllRolesAsync();
        return roles.Select(r => r.Name.ToString()).ToList();
    }

    public async Task SetActiveRoleAsync(string role)
    {
        var token = await _tokenRepo.GetLatestTokenAsync();
        if (token == null)
            throw new InvalidOperationException("No active session");

        token.Role = role;
        await _tokenRepo.UpdateTokenAsync(token);
    }

    public async Task<string> GetActiveRoleAsync()
    {
        var token = await _tokenRepo.GetLatestTokenAsync();
        return token?.Role ?? RoleType.Guest.ToString();
    }

    #endregion

    #region 📋 Session Management

    public async Task<AuthToken?> GetLatestAsync() =>
        await _tokenRepo.GetLatestTokenAsync();

    public async Task<SessionInfo> GetSessionAsync()
    {
        var token = await _tokenRepo.GetLatestTokenAsync();

        return new SessionInfo
        {
            UserId = token?.UserId ?? Guid.Empty,
            Email = "user@truckero.app",
            FullName = "Truckero User",
            ActiveRole = token?.Role ?? RoleType.Guest.ToString(),
            AvailableRoles = await GetAllRolesAsync(),
            TokenValid = token != null
        };
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepo.GetUserByEmailAsync(email);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _userRepo.GetUserByIdAsync(userId);
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var token = await _tokenRepo.GetLatestTokenAsync();
        if (token == null) return null;
        return await _userRepo.GetUserByIdAsync(token.UserId);
    }

    public async Task<AuthResponse> LoginToDeleteAccountAsync(string email, string password)
    {
        var user = await _userRepo.GetUserByEmailAsync(email);
        if (user == null || !_hashService.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        await _tokenRepo.DeleteAllTokensForUserAsync(user.Id);
        await _customerRepo.DeleteCustomerProfileChangesAsync(user.Id); // or driverRepo
        await _userRepo.DeleteUserAsync(user);

        return new AuthResponse { Success = true, ErrorMessage = ExceptionCodes.AccountDeleted };
    }

    public async Task<User?> GetUserByAccessToken(string accessToken)
    {
        return await _userRepo.GetUserByAccessTokenAsync(accessToken);
    }

    #endregion

}
