using Microsoft.AspNetCore.Mvc;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriverController : ControllerBase
{
    private readonly IDriverRepository _repo;

    public DriverController(IDriverRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<DriverProfile>> GetByUserId(Guid userId)
    {
        var profile = await _repo.GetByUserIdAsync(userId);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("{userId}/vehicles")]
    public async Task<ActionResult<List<Vehicle>>> GetVehicles(Guid userId)
    {
        var vehicles = await _repo.GetVehiclesAsync(userId);
        return Ok(vehicles);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DriverProfile profile)
    {
        await _repo.AddAsync(profile);
        await _repo.SaveChangesAsync();
        return CreatedAtAction(nameof(GetByUserId), new { userId = profile.UserId }, profile);
    }

    [HttpPost("vehicle")]
    public async Task<IActionResult> AddVehicle([FromBody] Vehicle vehicle)
    {
        await _repo.AddVehicleAsync(vehicle);
        await _repo.SaveChangesAsync();
        return Ok(vehicle);
    }

    [HttpDelete("vehicle/{vehicleId}")]
    public async Task<IActionResult> DeleteVehicle(Guid vehicleId)
    {
        await _repo.DeleteVehicleAsync(vehicleId);
        await _repo.SaveChangesAsync();
        return NoContent();
    }
}
