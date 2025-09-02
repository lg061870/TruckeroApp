using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Truckero.Core.Constants;
using Truckero.Core.DTOs.CustomerFlow;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Services;
using Truckero.Core.Services;
using static Truckero.Core.Constants.ExceptionCodes;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class FreightBidController : ControllerBase {
    private readonly IFreightBidService _freightBidService;
    private readonly IDriverBidService _driverBidService; // You need to add/implement this!

    public FreightBidController(
        IFreightBidService freightBidService,
        IDriverBidService driverBidService) {
        _freightBidService = freightBidService;
        _driverBidService = driverBidService;
    }

    // --- FreightBid endpoints ---

    [HttpPost]
    public async Task<ActionResult<FreightBidResponse>> CreateFreightBid([FromBody] FreightBidRequest request) {
        try {
            var result = await _freightBidService.CreateFreightBidAsync(request);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        } catch (ReferentialIntegrityException ex) {
            var error = new FreightBidResponse {
                Success = false,
                ErrorCode = ex.ErrorCode,
                Message = ex.Message
            };
            // Use switch expression to match your latest codes
            return ex.ErrorCode switch {
                FreightBidErrorCodes.CustomerNotFound or
                FreightBidErrorCodes.PreferredTruckTypeNotFound or
                FreightBidErrorCodes.AssignedTruckNotFound or
                FreightBidErrorCodes.AssignedDriverNotFound or
                FreightBidErrorCodes.PaymentMethodNotFound or
                FreightBidErrorCodes.UseTagNotFound or
                FreightBidErrorCodes.FreightBidUseTagNotFound =>
                    NotFound(error),
                _ => Conflict(error)
            };
        } catch (FreightBidException ex) {
            var error = new FreightBidResponse {
                Success = false,
                ErrorCode = ex.ErrorCode,
                Message = ex.Message
            };
            return Conflict(error);
        } catch (Exception) {
            return StatusCode(500, new FreightBidResponse {
                Success = false,
                ErrorCode = FreightBidErrorCodes.Unknown,
                Message = "Unhandled error occurred while creating freight bid."
            });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FreightBidResponse>> UpdateFreightBid(Guid id, [FromBody] FreightBidRequest request) {
        try {
            var result = await _freightBidService.UpdateFreightBidAsync(id, request);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        } catch (ReferentialIntegrityException ex) {
            var error = new FreightBidResponse {
                Success = false,
                ErrorCode = ex.ErrorCode,
                Message = ex.Message
            };
            return ex.ErrorCode switch {
                FreightBidErrorCodes.CustomerNotFound or
                FreightBidErrorCodes.PreferredTruckTypeNotFound or
                FreightBidErrorCodes.AssignedTruckNotFound or
                FreightBidErrorCodes.AssignedDriverNotFound or
                FreightBidErrorCodes.PaymentMethodNotFound or
                FreightBidErrorCodes.UseTagNotFound or
                FreightBidErrorCodes.FreightBidUseTagNotFound =>
                    NotFound(error),
                _ => Conflict(error)
            };
        } catch (FreightBidException ex) {
            var error = new FreightBidResponse {
                Success = false,
                ErrorCode = ex.ErrorCode,
                Message = ex.Message
            };
            return Conflict(error);
        } catch (Exception) {
            return StatusCode(500, new FreightBidResponse {
                Success = false,
                ErrorCode = FreightBidErrorCodes.Unknown,
                Message = "Unhandled error occurred while updating freight bid."
            });
        }
    }


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFreightBid(Guid id) {
        try {
            await _freightBidService.DeleteFreightBidAsync(id);
            return NoContent();
        } catch (ReferentialIntegrityException ex) {
            var error = new FreightBidResponse {
                Success = false,
                ErrorCode = ex.ErrorCode,
                Message = ex.Message
            };
            return NotFound(error);
        } catch (FreightBidException ex) {
            var error = new FreightBidResponse {
                Success = false,
                ErrorCode = ex.ErrorCode,
                Message = ex.Message
            };
            return Conflict(error);
        } catch (Exception) {
            return StatusCode(500, new FreightBidResponse {
                Success = false,
                ErrorCode = ExceptionCodes.FreightBidErrorCodes.Unknown,
                Message = "Unhandled error occurred while deleting freight bid."
            });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FreightBidDetailsResponse>> GetFreightBidDetails(Guid id) {
        var result = await _freightBidService.GetFreightBidDetailsAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FreightBidResponse>>> GetAllFreightBids() {
        var results = await _freightBidService.GetAllFreightBidsAsync();
        return Ok(results);
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<BidHistoryResponse>> GetFreightBidsByCustomerId(Guid customerId) {
        var response = await _freightBidService.GetBidHistoryAsync(customerId);
        return Ok(response);
    }

    // --- New: Find Drivers Status ---
    [HttpGet("{freightBidId:guid}/find-drivers-status")]
    public async Task<ActionResult<FindDriversStatusResponse>> GetFindDriversStatus(Guid freightBidId) {
        var result = await _freightBidService.GetFindDriversStatusAsync(freightBidId);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("{freightBidId:guid}/driver-bids")]
    public async Task<ActionResult<DriverBidResponse>> GetDriverBidsForFreightBid(Guid freightBidId) {
        var response = await _driverBidService.GetDriverBidsByFreightBidIdAsync(freightBidId);
        return Ok(response);
    }

    [HttpGet("driver-bid/{bidId:guid}")]
    public async Task<ActionResult<DriverBidResponse>> GetDriverBidDetails(Guid bidId) {
        var result = await _driverBidService.GetDriverBidByIdAsync(bidId);
        if (result == null || !result.Success)
            return NotFound(result ?? new DriverBidResponse { Success = false, Message = "Driver bid not found." });
        return Ok(result);
    }


    // --- New: Assign Driver ---
    [HttpPost("assign-driver")]
    public async Task<IActionResult> AssignDriver([FromBody] AssignDriverRequest request) {
        var result = await _freightBidService.AssignDriverAsync(request);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    // --- Existing: FreightBidUseTag endpoints ---

    [HttpPost("usetag")]
    public async Task<ActionResult<FreightBidUseTagResponse>> CreateFreightBidUseTag([FromBody] FreightBidUseTagRequest request) {
        try {
            var result = await _freightBidService.CreateFreightBidUseTagAsync(request);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        } catch (ReferentialIntegrityException ex) {
            var error = new FreightBidUseTagResponse {
                Success = false,
                ErrorCode = ex.ErrorCode,
                Message = ex.Message
            };
            // Only use codes you actually have!
            return ex.ErrorCode switch {
                FreightBidErrorCodes.NotFound or
                FreightBidErrorCodes.UseTagNotFound or
                FreightBidErrorCodes.FreightBidUseTagNotFound =>
                    NotFound(error),
                _ => Conflict(error)
            };
        } catch (FreightBidException ex) {
            var error = new FreightBidUseTagResponse {
                Success = false,
                ErrorCode = ex.ErrorCode,
                Message = ex.Message
            };
            return Conflict(error);
        } catch (Exception) {
            return StatusCode(500, new FreightBidUseTagResponse {
                Success = false,
                ErrorCode = FreightBidErrorCodes.Unknown,
                Message = "Unhandled error occurred while creating FreightBidUseTag."
            });
        }
    }

    [HttpDelete("usetag/{id:guid}")]
    public async Task<IActionResult> DeleteFreightBidUseTag(Guid id) {
        try {
            await _freightBidService.DeleteFreightBidUseTagAsync(id);
            return NoContent();
        } catch (ReferentialIntegrityException ex) {
            var error = new FreightBidUseTagResponse {
                Success = false,
                ErrorCode = ex.ErrorCode,
                Message = ex.Message
            };
            return NotFound(error);
        } catch (FreightBidException ex) {
            var error = new FreightBidUseTagResponse {
                Success = false,
                ErrorCode = ex.ErrorCode,
                Message = ex.Message
            };
            return Conflict(error);
        } catch (Exception) {
            return StatusCode(500, new FreightBidUseTagResponse {
                Success = false,
                ErrorCode = ExceptionCodes.FreightBidErrorCodes.Unknown,
                Message = "Unhandled error occurred while deleting FreightBidUseTag."
            });
        }
    }

    [HttpGet("usetag/{id:guid}")]
    public async Task<ActionResult<FreightBidUseTagResponse>> GetFreightBidUseTagById(Guid id) {
        var result = await _freightBidService.GetFreightBidUseTagByIdAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpGet("usetag/byfreightbid/{freightBidId:guid}")]
    public async Task<ActionResult<IReadOnlyList<FreightBidUseTagResponse>>> GetFreightBidUseTagsByFreightBidId(Guid freightBidId) {
        var results = await _freightBidService.GetFreightBidUseTagsByFreightBidIdAsync(freightBidId);
        return Ok(results);
    }

    [HttpGet("usetag/byusetag/{useTagId:guid}")]
    public async Task<ActionResult<IReadOnlyList<FreightBidUseTagResponse>>> GetFreightBidUseTagsByUseTagId(Guid useTagId) {
        var results = await _freightBidService.GetFreightBidUseTagsByUseTagIdAsync(useTagId);
        return Ok(results);
    }
}
