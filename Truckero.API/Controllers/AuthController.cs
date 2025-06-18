using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Services;
using Truckero.Infrastructure.Extensions;
using Truckero.Infrastructure.Services;

namespace Truckero.API.Controllers;

[ApiController]
[Route("auth")]
[Produces(MediaTypeNames.Application.Json)]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuthTokenRepository _authTokenRepo;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IAuthTokenRepository authTokenRepo, ILogger<AuthController> logger)
    {
        _authService = authService;
        _authTokenRepo = authTokenRepo;
        _logger = logger;
    }

    // 🔐 Registration + Login

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var response = await _authService.RegisterUserAsync(request);
        return Created(string.Empty, response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthLoginRequest request)
    {
        try
        {
            var response = await _authService.LoginUserAsync(request);
            _logger.LogInformation("User {Email} logged in.", request.Email);
            return Ok(response);
        }
        catch (LoginStepException ex)
        {
            return BadRequest(new { message = ex.Message, stepCode = ex.StepCode });
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Unauthorized login attempt for {Email}", request.Email);
            return Unauthorized();
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] Guid userId)
    {
        await _authService.LogoutUserAsync(userId);
        _logger.LogInformation("User {UserId} logged out.", userId);
        return NoContent();
    }

    // 🔁 Token Handling

    [HttpPost("exchange")]
    public async Task<IActionResult> ExchangeToken([FromBody] TokenRequest request)
    {
        var response = await _authService.ExchangeTokenAsync(request);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var response = await _authService.RefreshAccessTokenAsync(request);
            return Ok(response);
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = "Invalid or expired refresh token" });
        }
    }

    [HttpGet("validate")]
    public async Task<IActionResult> Validate([FromQuery] string accessToken)
    {
        var isValid = await _authService.ValidateTokenAsync(accessToken);
        return isValid ? Ok() : Unauthorized();
    }

    // 🔑 Password Reset

    [HttpPost("password/request-reset")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _authService.RequestPasswordResetAsync(request);
            return Accepted(new { message = "Password reset email sent." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = $"No account found for {request.Email}" }); // 👈 More informative
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during password reset request.");
            return StatusCode(500, new { message = "Internal error while processing your request." });
        }
    }

    [HttpPost("password/confirm-reset")]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] PasswordResetConfirmRequest request)
    {
        try
        {
            await _authService.ConfirmPasswordResetAsync(request);
            return Ok(new { message = "Password successfully reset." });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            // Optionally log the exception here
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }


    #region 🧭 Role Management

    [Authorize]
    [HttpGet("role/active")]
    public async Task<IActionResult> GetActiveRole()
    {
        var userId = User.GetUserId();
        var token = await _authTokenRepo.GetTokenByUserIdAsync(userId);
        return Ok(token?.Role ?? "Guest");
    }

    [Authorize]
    [HttpGet("role/all")]
    public IActionResult GetAllRoles()
    {
        return Ok(new[] { "Customer", "Driver", "StoreClerk" });
    }

    [Authorize]
    [HttpPost("role/set")]
    public async Task<IActionResult> SetActiveRole([FromBody] string role)
    {
        var validRoles = new[] { "Customer", "Driver", "StoreClerk" };
        if (!validRoles.Contains(role))
            return BadRequest($"Invalid role: {role}");

        var userId = User.GetUserId();
        var token = await _authTokenRepo.GetTokenByUserIdAsync(userId);
        if (token == null) return NotFound("User token not found.");

        token.Role = role;
        await _authTokenRepo.UpdateTokenAsync(token);

        return Ok(new { message = $"Role set to {role}" });
    }

    #endregion

    // 📋 Session Info

    [Authorize]
    [HttpGet("session")]
    public async Task<IActionResult> GetSession()
    {
        var userId = User.GetUserId();
        var email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "user@example.com";
        var fullName = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "Test User";

        var token = await _authTokenRepo.GetTokenByUserIdAsync(userId);
        var role = token?.Role ?? "Guest";

        return Ok(new SessionInfo
        {
            UserId = userId,
            Email = email,
            FullName = fullName,
            ActiveRole = role,
            AvailableRoles = new List<string> { "Customer", "Driver", "StoreClerk" },
            TokenValid = true
        });
    }

    // 🧪 Debug/Diagnostics Endpoint

    [Authorize]
    [HttpGet("token/latest")]
    public async Task<IActionResult> GetLatestToken()
    {
        var token = await _authTokenRepo.GetLatestTokenAsync();
        if (token == null)
            return NotFound();

        return Ok(token);
    }

    [HttpGet("user/by-email")]
    public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { error = "Email is required to retrieve user info." });

        var user = await _authService.GetUserByEmailAsync(email);

        if (user == null)
            return NotFound(new { error = "User not found." });

        return Ok(user);
    }


    [Authorize]
    [HttpGet("user/me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userId, out var guid))
            return Unauthorized();

        var user = await _authService.GetUserByIdAsync(guid);
        return user != null ? Ok(user) : NotFound();
    }


    [Authorize]
    [HttpGet("user/by-id")]
    public async Task<IActionResult> GetUserById([FromQuery] Guid userId)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpPost("login-delete")]
    public async Task<IActionResult> LoginToDelete([FromBody] LoginRequest payload)
    {
        var result = await _authService.LoginToDeleteAccountAsync(payload.Email, payload.Password);
        return Ok(result);
    }

    [HttpGet("auth/user/by-access-token")]
    public async Task<IActionResult> GetUserByAccessToken([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("Access token is required.");

        var user = await _authService.GetUserByAccessToken(token);
        return user != null ? Ok(user) : NotFound();
    }

}
