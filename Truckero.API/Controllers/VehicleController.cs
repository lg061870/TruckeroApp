using Microsoft.AspNetCore.Mvc;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehicleController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepo;

    public VehicleController(IVehicleRepository vehicleRepo)
    {
        _vehicleRepo = vehicleRepo;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterVehicle([FromBody] Truck vehicle)
    {
        try
        {
            await _vehicleRepo.AddAsync(vehicle);
            return Ok(new { success = true, message = "Vehicle registered successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while registering the vehicle." });
        }
    }

    [HttpDelete("{vehicleId}")]
    public async Task<IActionResult> DeleteVehicle(Guid vehicleId)
    {
        try
        {
            await _vehicleRepo.DeleteAsync(vehicleId);
            return Ok(new { success = true, message = "Vehicle deleted successfully." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Vehicle not found." });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while deleting the vehicle." });
        }
    }
}
