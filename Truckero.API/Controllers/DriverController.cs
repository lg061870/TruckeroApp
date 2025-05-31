using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // 🔒 All endpoints require authentication
public class DriverController : ControllerBase
{
    private readonly IDriverRepository _repo;

    public DriverController(IDriverRepository repo)
    {
        _repo = repo;
    }

    // Helper to get the authenticated user's ID (assumes claims-based identity)
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        throw new UnauthorizedAccessException("Invalid or missing user ID claim.");
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<DriverProfile>> GetByUserId(Guid userId)
    {
        if (userId != GetCurrentUserId())
            return Forbid();

        var profile = await _repo.GetByUserIdAsync(userId);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("{userId}/vehicles")]
    public async Task<ActionResult<List<Vehicle>>> GetVehicles(Guid userId)
    {
        if (userId != GetCurrentUserId())
            return Forbid();

        var vehicles = await _repo.GetVehiclesAsync(userId);
        return Ok(vehicles);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DriverProfile profile)
    {
        if (profile.UserId != GetCurrentUserId())
            return Forbid();

        await _repo.AddDriverProfileAsync(profile);
        await _repo.SaveDriverProfileChangesAsync();
        return CreatedAtAction(nameof(GetByUserId), new { userId = profile.UserId }, profile);
    }

    [HttpPost("vehicle")]
    public async Task<IActionResult> AddVehicle([FromBody] Vehicle vehicle)
    {
        if (vehicle.DriverProfile.UserId != GetCurrentUserId())
            return Forbid();

        await _repo.AddVehicleAsync(vehicle);
        await _repo.SaveDriverProfileChangesAsync();
        return Ok(vehicle);
    }

    [HttpDelete("vehicle/{vehicleId}")]
    public async Task<IActionResult> DeleteVehicle(Guid vehicleId)
    {
        // To enforce per-user security, you would first look up the vehicle to check ownership:
        // var vehicle = await _repo.GetVehicleByIdAsync(vehicleId);
        // if (vehicle == null || vehicle.UserId != GetCurrentUserId()) return Forbid();

        await _repo.DeleteVehicleAsync(vehicleId);
        await _repo.SaveDriverProfileChangesAsync();
        return NoContent();
    }
}
