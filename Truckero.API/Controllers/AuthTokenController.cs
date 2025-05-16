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

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var token = await _tokenRepo.GetByUserIdAsync(userId);
        return token is null ? NotFound() : Ok(token);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AuthToken token)
    {
        await _tokenRepo.AddAsync(token);
        return CreatedAtAction(nameof(GetByUserId), new { userId = token.UserId }, token);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] AuthToken token)
    {
        await _tokenRepo.UpdateAsync(token);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] AuthToken token)
    {
        await _tokenRepo.DeleteAsync(token);
        return NoContent();
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest()
    {
        var token = await _tokenRepo.GetLatestAsync();
        return token is null ? NotFound() : Ok(token);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] string refreshToken)
    {
        await _tokenRepo.RevokeAsync(refreshToken);
        return Ok();
    }
}