using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Trucks;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Services;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/driver")]
[Authorize]
public class DriverController : ControllerBase
{
    private readonly ITruckService _truckService;
    private readonly ILogger<DriverController> _logger;

    public DriverController(ITruckService truckService, ILogger<DriverController> logger)
    {
        _truckService = truckService;
        _logger = logger;
    }

    // Helper to get the authenticated user's ID (assumes claims-based identity)
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        throw new UnauthorizedAccessException("Invalid or missing user ID claim.");
    }

    [HttpGet("{userId}/trucks")]
    public async Task<ActionResult<List<Truck>>> GetDriverTrucks(Guid userId)
    {
        try
        {
            var trucks = await _truckService.GetDriverTrucksAsync(userId);
            return Ok(trucks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching trucks for driver {UserId}", userId);
            return StatusCode(500, "Failed to retrieve truck information");
        }
    }

    [HttpPost("{userId}/trucks")]
    public async Task<ActionResult<TruckResponseDto>> AddDriverTruck(Guid userId, [FromBody] TruckRequestDto truck)
    {
        try
        {
            var result = await _truckService.AddDriverTruckAsync(userId, truck);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding truck for driver {UserId}", userId);
            return StatusCode(500, "Failed to add truck");
        }
    }

    [HttpPut("{userId}/trucks/{truckId}")]
    public async Task<ActionResult<TruckResponseDto>> UpdateDriverTruck(Guid userId, Guid truckId, [FromBody] TruckRequestDto truck)
    {
        try
        {
            var result = await _truckService.UpdateDriverTruckAsync(userId, truckId, truck);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating truck {TruckId} for driver {UserId}", truckId, userId);
            return StatusCode(500, "Failed to update truck");
        }
    }

    [HttpDelete("{userId}/trucks/{truckId}")]
    public async Task<ActionResult<TruckResponseDto>> DeleteDriverTruck(Guid userId, Guid truckId)
    {
        try
        {
            var result = await _truckService.DeleteDriverTruckAsync(userId, truckId);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting truck {TruckId} for driver {UserId}", truckId, userId);
            return StatusCode(500, "Failed to delete truck");
        }
    }
}
