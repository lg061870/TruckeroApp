using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Truckero.Core.Constants;
using Truckero.Core.DTOs.CustomerFlow;
using Truckero.Core.Exceptions;
using Truckero.Core.Services;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class FreightBidController : ControllerBase {
    private readonly IFreightBidService _freightBidService;

    public FreightBidController(IFreightBidService freightBidService) {
        _freightBidService = freightBidService;
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
            return ex.ErrorCode switch {
                ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound => NotFound(error),
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
                ErrorCode = ExceptionCodes.FreightBidErrorCodes.Unknown,
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
                ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound => NotFound(error),
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
                ErrorCode = ExceptionCodes.FreightBidErrorCodes.Unknown,
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
    public async Task<ActionResult<FreightBidResponse>> GetFreightBidById(Guid id) {
        var result = await _freightBidService.GetFreightBidByIdAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FreightBidResponse>>> GetAllFreightBids() {
        var results = await _freightBidService.GetAllFreightBidsAsync();
        return Ok(results);
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IReadOnlyList<FreightBidResponse>>> GetFreightBidsByCustomerId(Guid customerId) {
        var results = await _freightBidService.GetFreightBidsByCustomerIdAsync(customerId);
        return Ok(results);
    }

    // --- FreightBidUseTag endpoints ---

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
            return ex.ErrorCode switch {
                ExceptionCodes.FreightBidErrorCodes.ForeignKeyNotFound => NotFound(error),
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
                ErrorCode = ExceptionCodes.FreightBidErrorCodes.Unknown,
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
