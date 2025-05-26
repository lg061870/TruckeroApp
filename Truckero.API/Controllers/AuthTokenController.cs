using Microsoft.AspNetCore.Mvc;
using Truckero.Core.Interfaces;

namespace Truckero.API.Controllers;

[ApiController]
[Route("tokens")]
[Produces("application/json")]
public class AuthTokenController : ControllerBase
{
    private readonly IAuthTokenRepository _tokenRepo;

    public AuthTokenController(IAuthTokenRepository tokenRepo)
    {
        _tokenRepo = tokenRepo;
    }

    [HttpGet("validate")]
    public async Task<IActionResult> ValidateToken([FromQuery] string token)
    {
        var authToken = await _tokenRepo.GetByAccessTokenByAccessTokenKeyAsync(token);
        if (authToken == null)
            return Ok(new { valid = false, reason = "accesstoken_not_found" });
        if (authToken.ExpiresAt < DateTime.UtcNow)
            return Ok(new { valid = false, reason = "accesstoken_expired" });
        if (authToken.User?.EmailVerified != true)
            return Ok(new { valid = false, reason = "email_not_verified" });

        return Ok(new { valid = true });
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var token = await _tokenRepo.GetByTokenByUserIdAsync(userId);
        return token is null ? NotFound() : Ok(token);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AuthToken token)
    {
        await _tokenRepo.AddTokenAsync(token);
        return CreatedAtAction(nameof(GetByUserId), new { userId = token.UserId }, token);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] AuthToken token)
    {
        await _tokenRepo.UpdateTokenAsync(token);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] AuthToken token)
    {
        await _tokenRepo.DeleteTokenAsync(token);
        return NoContent();
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest()
    {
        var token = await _tokenRepo.GetLatestTokenAsync();
        return token is null ? NotFound() : Ok(token);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] string refreshToken)
    {
        await _tokenRepo.RevokeRefreshTokenAsync(refreshToken);
        return Ok();
    }
}