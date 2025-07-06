using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Truckero.Core.DTOs.CustomerFlow;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Services;
using Truckero.Core.Constants;

namespace Truckero.API.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class DriverBidController : ControllerBase {
        private readonly IDriverBidService _driverBidService;

        public DriverBidController(IDriverBidService driverBidService) {
            _driverBidService = driverBidService;
        }

        [HttpPost]
        public async Task<ActionResult<DriverBidResponse>> Create([FromBody] DriverBidRequest request) {
            try {
                var result = await _driverBidService.CreateDriverBidAsync(request);
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            } catch (ReferentialIntegrityException ex) {
                var error = new DriverBidResponse {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message
                };
                return ex.ErrorCode switch {
                    ExceptionCodes.DriverBidErrorCodes.ForeignKeyNotFound => NotFound(error),
                    _ => Conflict(error)
                };
            } catch (DriverBidException ex) {
                var error = new DriverBidResponse {
                    Success = false,
                    ErrorCode = ex.Code,
                    Message = ex.Message
                };
                return Conflict(error);
            } catch (Exception) {
                return StatusCode(500, new DriverBidResponse {
                    Success = false,
                    ErrorCode = ExceptionCodes.DriverBidErrorCodes.Unknown,
                    Message = "Unhandled error occurred while creating driver bid."
                });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<DriverBidResponse>> Update(Guid id, [FromBody] DriverBidRequest request) {
            try {
                var result = await _driverBidService.UpdateDriverBidAsync(id, request);
                if (!result.Success)
                    return NotFound(result);

                return Ok(result);
            } catch (ReferentialIntegrityException ex) {
                var error = new DriverBidResponse {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message
                };
                return ex.ErrorCode switch {
                    ExceptionCodes.DriverBidErrorCodes.ForeignKeyNotFound => NotFound(error),
                    _ => Conflict(error)
                };
            } catch (DriverBidException ex) {
                var error = new DriverBidResponse {
                    Success = false,
                    ErrorCode = ex.Code,
                    Message = ex.Message
                };
                return Conflict(error);
            } catch (Exception) {
                return StatusCode(500, new DriverBidResponse {
                    Success = false,
                    ErrorCode = ExceptionCodes.DriverBidErrorCodes.Unknown,
                    Message = "Unhandled error occurred while updating driver bid."
                });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) {
            try {
                var result = await _driverBidService.DeleteDriverBidAsync(id);
                if (!result.Success)
                    return NotFound(result);

                return NoContent();
            } catch (ReferentialIntegrityException ex) {
                var error = new DriverBidResponse {
                    Success = false,
                    ErrorCode = ex.ErrorCode,
                    Message = ex.Message
                };
                return NotFound(error);
            } catch (DriverBidException ex) {
                var error = new DriverBidResponse {
                    Success = false,
                    ErrorCode = ex.Code,
                    Message = ex.Message
                };
                return Conflict(error);
            } catch (Exception) {
                return StatusCode(500, new DriverBidResponse {
                    Success = false,
                    ErrorCode = ExceptionCodes.DriverBidErrorCodes.Unknown,
                    Message = "Unhandled error occurred while deleting driver bid."
                });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<DriverBidResponse>> GetById(Guid id) {
            var result = await _driverBidService.GetDriverBidByIdAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("freight/{freightBidId:guid}")]
        public async Task<ActionResult<DriverBidResponse>> GetByFreightBidId(Guid freightBidId) {
            var result = await _driverBidService.GetDriverBidsByFreightBidIdAsync(freightBidId);
            return Ok(result);
        }

        [HttpGet("driver/{driverId:guid}")]
        public async Task<ActionResult<DriverBidResponse>> GetByDriverId(Guid driverId) {
            var result = await _driverBidService.GetDriverBidsByDriverIdAsync(driverId);
            return Ok(result);
        }
    }
}
