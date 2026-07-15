using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Truckero.Core.DTOs;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Services;
using Truckero.Core.Constants;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class StoreController : ControllerBase
{
    private readonly IStoreService _storeService;

    public StoreController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpPost]
    public async Task<ActionResult<StoreResponse>> Create([FromBody] StoreRequest request)
    {
        try
        {
            var result = await _storeService.CreateStoreAsync(request);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (ReferentialIntegrityException ex)
        {
            var error = new StoreResponse
            {
                Success = false,
                ErrorCode = ex.ErrorCode,
                Message = ex.Message
            };
            return ex.ErrorCode switch
            {
                ExceptionCodes.StoreErrorCodes.ForeignKeyNotFound => NotFound(error),
                _ => Conflict(error)
            };
        }
        catch (StoreException ex)
        {
            var error = new StoreResponse
            {
                Success = false,
                ErrorCode = ex.Code,
                Message = ex.Message
            };
            return Conflict(error);
        }
        catch (Exception)
        {
            return StatusCode(500, new StoreResponse
            {
                Success = false,
                ErrorCode = ExceptionCodes.StoreErrorCodes.Unknown,
                Message = "Unhandled error occurred while creating store."
            });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StoreResponse>> Update(Guid id, [FromBody] StoreRequest request)
    {
        try
        {
            var result = await _storeService.UpdateStoreAsync(id, request);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
        catch (ReferentialIntegrityException ex)
        {
            var error = new StoreResponse
            {
                Success = false,
                ErrorCode = ex.ErrorCode,
                Message = ex.Message
            };
            return ex.ErrorCode switch
            {
                ExceptionCodes.StoreErrorCodes.ForeignKeyNotFound => NotFound(error),
                _ => Conflict(error)
            };
        }
        catch (StoreException ex)
        {
            var error = new StoreResponse
            {
                Success = false,
                ErrorCode = ex.Code,
                Message = ex.Message
            };
            return Conflict(error);
        }
        catch (Exception)
        {
            return StatusCode(500, new StoreResponse
            {
                Success = false,
                ErrorCode = ExceptionCodes.StoreErrorCodes.Unknown,
                Message = "Unhandled error occurred while updating store."
            });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _storeService.DeleteStoreAsync(id);
            if (!result.Success)
                return NotFound(result);

            return NoContent();
        }
        catch (ReferentialIntegrityException ex)
        {
            var error = new StoreResponse
            {
                Success = false,
                ErrorCode = ex.ErrorCode,
                Message = ex.Message
            };
            return NotFound(error);
        }
        catch (StoreException ex)
        {
            var error = new StoreResponse
            {
                Success = false,
                ErrorCode = ex.Code,
                Message = ex.Message
            };
            return Conflict(error);
        }
        catch (Exception)
        {
            return StatusCode(500, new StoreResponse
            {
                Success = false,
                ErrorCode = ExceptionCodes.StoreErrorCodes.Unknown,
                Message = "Unhandled error occurred while deleting store."
            });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StoreResponse>> GetById(Guid id)
    {
        var result = await _storeService.GetStoreByIdAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpGet("company/{companyId:guid}")]
    public async Task<ActionResult<StoreListResponse>> GetByCompanyId(Guid companyId)
    {
        var result = await _storeService.GetStoresByCompanyIdAsync(companyId);
        return Ok(result);
    }
}
