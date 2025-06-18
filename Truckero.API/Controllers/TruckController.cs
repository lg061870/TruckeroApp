using Microsoft.AspNetCore.Mvc;
using Truckero.Core.DTOs.Trucks;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Services;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TruckController : ControllerBase
{
    private readonly ITruckService _truckService;

    public TruckController(ITruckService truckService)
    {
        _truckService = truckService;
    }

    [HttpGet("makes")]
    public async Task<ActionResult<List<TruckMake>>> GetTruckMakes()
        => Ok(await _truckService.GetTruckMakesAsync());

    [HttpGet("models")]
    public async Task<ActionResult<List<TruckModel>>> GetTruckModels([FromQuery] Guid? makeId = null)
        => Ok(await _truckService.GetTruckModelsAsync(makeId));

    [HttpGet("categories")]
    public async Task<ActionResult<List<TruckCategory>>> GetTruckCategories()
        => Ok(await _truckService.GetTruckCategoriesAsync());

    [HttpGet("bedtypes")]
    public async Task<ActionResult<List<BedType>>> GetBedTypes()
        => Ok(await _truckService.GetBedTypesAsync());

    // GET: api/truck/driver/{userId}
    [HttpGet("driver/{userId}")]
    public async Task<ActionResult<List<Truck>>> GetDriverTrucks(Guid userId)
    {
        var trucks = await _truckService.GetDriverTrucksAsync(userId);
        return Ok(trucks);
    }

    [HttpPost("driver/{userId}")]
    public async Task<ActionResult<TruckResponseDto>> AddDriverTruck(Guid userId, [FromBody] TruckRequestDto truck) {
        try {
            var result = await _truckService.AddDriverTruckAsync(userId, truck);
            if (!result.Success)
                // Application-level error (e.g. validation), sent as BadRequest
                return BadRequest(result);

            return Ok(result);
        } catch (TruckStepException tex) {
            // You can be more granular with your codes here
            // For example: "DRIVER_PROFILE_NOT_FOUND", "TRUCK_MODEL_INVALID", etc.

            var error = new TruckResponseDto {
                Success = false,
                ErrorCode = tex.StepCode,
                Message = tex.Message
            };

            // Return 409 Conflict for referential integrity
            return tex.StepCode switch {
                "DRIVER_PROFILE_NOT_FOUND" => NotFound(error),
                "TRUCK_MODEL_NOT_FOUND" => NotFound(error),
                "TRUCK_ALREADY_EXISTS" => Conflict(error),
                _ => Conflict(error)
            };
        } catch (Exception ex) {
            // Unexpected/unhandled errors – log as needed
            var error = new TruckResponseDto {
                Success = false,
                ErrorCode = "UNHANDLED_EXCEPTION",
                Message = "An unexpected error occurred while adding the truck."
            };
            return StatusCode(500, error);
        }
    }


    // PUT: api/truck/driver/{userId}/{truckId}
    [HttpPut("driver/{userId}/{truckId}")]
    public async Task<ActionResult<TruckResponseDto>> UpdateDriverTruck(Guid userId, Guid truckId, [FromBody] TruckRequestDto truck)
    {
        var result = await _truckService.UpdateDriverTruckAsync(userId, truckId, truck);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    // DELETE: api/truck/driver/{userId}/{truckId}
    [HttpDelete("driver/{userId}/{truckId}")]
    public async Task<ActionResult<TruckResponseDto>> DeleteDriverTruck(Guid userId, Guid truckId)
    {
        var result = await _truckService.DeleteDriverTruckAsync(userId, truckId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
