using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Truckero.Core.Constants;
using Truckero.Core.Interfaces;
using Truckero.Core.Interfaces.Repositories;

namespace Truckero.API.Auth;

public class AuthTokenSecurityHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IAuthTokenRepository _authTokenRepository;
    private string? _lastFailureReason; // Add this field

    public AuthTokenSecurityHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IAuthTokenRepository authTokenRepository)
        : base(options, logger, encoder)
    {
        _authTokenRepository = authTokenRepository;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? token = null;
        try
        {
            if (Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    token = authHeader.Substring("Bearer ".Length).Trim();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error parsing Authorization header.");
            _lastFailureReason = ExceptionCodes.InvalidHttpRequestSecurityHeader;
            return AuthenticateResult.Fail(_lastFailureReason);
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            _lastFailureReason = ExceptionCodes.AccessTokenIsBlank;
            return AuthenticateResult.Fail(_lastFailureReason);
        }

        var authToken = await _authTokenRepository.GetAccessTokenByAccessTokenKeyAsync(token);
        if (authToken == null)
        {
            _lastFailureReason = ExceptionCodes.AccessTokenNotFound;
            return AuthenticateResult.Fail(_lastFailureReason);
        }

        if (authToken.ExpiresAt < DateTime.UtcNow)
        {
            _lastFailureReason = ExceptionCodes.AccessTokenExpired;
            return AuthenticateResult.Fail(_lastFailureReason);
        }

        // Clear failure reason on success
        _lastFailureReason = null;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, authToken.UserId.ToString()),
            new Claim(ClaimTypes.Name, authToken.User?.Email ?? "unknown"),
            new Claim(ClaimTypes.Email, authToken.User?.Email ?? "unknown"),
            new Claim(ClaimTypes.Role, authToken.Role ?? "Guest"),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var errorCode = _lastFailureReason ?? "Unauthorized";
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = "application/json";

        var payload = new
        {
            error = errorCode
        };


        Response.Headers.Add("X-Error-Code", errorCode); // Optional: custom headers

        await Response.WriteAsync(JsonSerializer.Serialize(payload));

    }
}