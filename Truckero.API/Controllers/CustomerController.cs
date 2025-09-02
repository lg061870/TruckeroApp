using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // 🔒 Require authentication for all endpoints
public class CustomerController : ControllerBase
{
    private readonly ICustomerProfileRepository _repo;

    public CustomerController(ICustomerProfileRepository repo)
    {
        _repo = repo;
    }

    // Helper to get the current authenticated user's ID (assumes claims-based identity)
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        throw new UnauthorizedAccessException("Invalid or missing user ID claim.");
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<CustomerProfile>> GetByCustomerProfileByUserId(Guid userId)
    {
        // Ownership check: ensure user is requesting their own profile
        if (userId != GetCurrentUserId())
            return Forbid();

        var profile = await _repo.GetCustomerProfileByUserIdAsync(userId);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerProfile profile)
    {
        // Ownership check: ensure user is creating their own profile
        if (profile.UserId != GetCurrentUserId())
            return Forbid();

        await _repo.AddCustomerProfileAsync(profile);
        await _repo.SaveCustomerProfileChangesAsync();
        return CreatedAtAction(nameof(GetByCustomerProfileByUserId), new { userId = profile.UserId }, profile);
    }
}
