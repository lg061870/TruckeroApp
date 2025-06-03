using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Truckero.Core.DTOs.Common;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Services;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/driver")]
[Authorize]
public class DriverController : ControllerBase
{
    private readonly IOnboardingService _onboardingService;
    private readonly ILogger<DriverController> _logger;

    public DriverController(IOnboardingService onboardingService, ILogger<DriverController> logger)
    {
        _onboardingService = onboardingService;
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
            var trucks = await _onboardingService.GetDriverTrucksAsync(userId);
            return Ok(trucks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching trucks for driver {UserId}", userId);
            return StatusCode(500, "Failed to retrieve truck information");
        }
    }

    [HttpPost("{userId}/trucks")]
    public async Task<ActionResult<OperationResult>> AddDriverTruck(Guid userId, [FromBody] Truck truck)
    {
        try
        {
            var result = await _onboardingService.AddDriverTruckAsync(userId, truck);
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
    public async Task<ActionResult<OperationResult>> UpdateDriverTruck(Guid userId, Guid truckId, [FromBody] Truck truck)
    {
        if (truck.Id != truckId)
            return BadRequest("Truck ID mismatch");
            
        try
        {
            var result = await _onboardingService.UpdateDriverTruckAsync(userId, truck);
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
    public async Task<ActionResult<OperationResult>> DeleteDriverTruck(Guid userId, Guid truckId)
    {
        try
        {
            var result = await _onboardingService.DeleteDriverTruckAsync(userId, truckId);
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
