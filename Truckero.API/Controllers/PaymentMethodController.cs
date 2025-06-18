using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Billing;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Services;
using Truckero.Infrastructure.Services;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentMethodController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly ILogger<PaymentMethodController> _logger;

    public PaymentMethodController(IPaymentMethodService paymentMethodService, ILogger<PaymentMethodController> logger)
    {
        _paymentMethodService = paymentMethodService ?? throw new ArgumentNullException(nameof(paymentMethodService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // GET: api/paymentmethod/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<PaymentMethod>>> GetPaymentMethodsByUserId(Guid userId)
    {
        // TODO: Add authorization to ensure requesting user is userId or an admin
        var methods = await _paymentMethodService.GetPaymentMethodsByUserIdAsync(userId);
        return Ok(methods);
    }

    // GET: api/paymentmethod/{paymentMethodId}/user/{userId} 
    // (Added userId for ownership check, could also be implicit from auth token)
    [HttpGet("{paymentMethodId}/user/{userId}")]
    public async Task<ActionResult<PaymentMethod>> GetPaymentMethodById(Guid paymentMethodId, Guid userId)
    {
        var method = await _paymentMethodService.GetPaymentMethodByIdAsync(paymentMethodId, userId);
        if (method == null) return NotFound(OperationResult.Failed("Payment method not found or access denied.", "PM_NOT_FOUND"));
        return Ok(method);
    }

    // GET: api/paymentmethod/user/{userId}/default
    [HttpGet("user/{userId}/default")]
    public async Task<ActionResult<PaymentMethod>> GetDefaultPaymentMethodByUserId(Guid userId)
    {
        var method = await _paymentMethodService.GetDefaultPaymentMethodByUserIdAsync(userId);
        return Ok(method); // Ok(null) is acceptable if no default is set
    }

    // POST: api/paymentmethod/user/{userId}
    [HttpPost("user/{userId}")]
    public async Task<ActionResult<OperationResult<PaymentMethod>>> AddPaymentMethod(Guid userId, [FromBody] PaymentMethodDto paymentMethodDto)
    {
        if (!ModelState.IsValid) return BadRequest(OperationResult<PaymentMethod>.Failed("Invalid request data.", "VALIDATION_ERROR"));
        
        try
        {
            var result = await _paymentMethodService.AddPaymentMethodAsync(userId, paymentMethodDto);
            if (!result.Success)
            {
                if (result.ErrorCode == "USER_NOT_FOUND" || result.ErrorCode == "INVALID_PM_TYPE") return NotFound(result);
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetPaymentMethodById), new { paymentMethodId = result.Data!.Id, userId = userId }, result);
        }
        catch (PaymentMethodStepException pmEx)
        {
            _logger.LogWarning(pmEx, "PaymentMethodStepException: {ErrorCode} - {Message}", pmEx.StepCode, pmEx.Message);
            return StatusCode(500, OperationResult<PaymentMethod>.Failed(pmEx.Message, pmEx.StepCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error adding payment method for user {UserId}", userId);
            return StatusCode(500, OperationResult<PaymentMethod>.Failed("An unexpected server error occurred.", "UNHANDLED_EXCEPTION"));
        }
    }

    // PUT: api/paymentmethod/user/{userId}/{paymentMethodId}
    [HttpPut("user/{userId}/{paymentMethodId}")]
    public async Task<ActionResult<OperationResult<PaymentMethod>>> UpdatePaymentMethod(Guid userId, Guid paymentMethodId, [FromBody] PaymentMethodDto paymentMethodDto)
    {
        if (!ModelState.IsValid) return BadRequest(OperationResult<PaymentMethod>.Failed("Invalid request data.", "VALIDATION_ERROR"));
        if (paymentMethodId != paymentMethodDto.Id && paymentMethodDto.Id != Guid.Empty) 
            return BadRequest(OperationResult<PaymentMethod>.Failed("Payment method ID mismatch.", "ID_MISMATCH"));
        if(paymentMethodDto.Id == Guid.Empty) paymentMethodDto.Id = paymentMethodId;

        try
        {
            var result = await _paymentMethodService.UpdatePaymentMethodAsync(userId, paymentMethodId, paymentMethodDto);
            if (!result.Success)
            {
                if (result.ErrorCode == "PM_NOT_FOUND" || result.ErrorCode == "INVALID_PM_TYPE") return NotFound(result);
                return BadRequest(result);
            }
            return Ok(result);
        }
        catch (PaymentMethodStepException pmEx)
        {
             _logger.LogWarning(pmEx, "PaymentMethodStepException: {ErrorCode} - {Message}", pmEx.StepCode, pmEx.Message);
            return StatusCode(500, OperationResult<PaymentMethod>.Failed(pmEx.Message, pmEx.StepCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating payment method {PaymentMethodId}", paymentMethodId);
            return StatusCode(500, OperationResult<PaymentMethod>.Failed("An unexpected server error occurred.", "UNHANDLED_EXCEPTION"));
        }
    }

    // DELETE: api/paymentmethod/user/{userId}/{paymentMethodId}
    [HttpDelete("user/{userId}/{paymentMethodId}")]
    public async Task<ActionResult<OperationResult>> DeletePaymentMethod(Guid userId, Guid paymentMethodId)
    {
        try
        {
            var result = await _paymentMethodService.DeletePaymentMethodAsync(userId, paymentMethodId);
            if (!result.Success)
            {
                if (result.ErrorCode == "PM_NOT_FOUND") return NotFound(result);
                if (result.ErrorCode == "CANNOT_DELETE_DEFAULT_PM") return Conflict(result);
                return BadRequest(result);
            }
            return Ok(result);
        }
         catch (PaymentMethodStepException pmEx)
        {
            _logger.LogWarning(pmEx, "PaymentMethodStepException: {ErrorCode} - {Message}", pmEx.StepCode, pmEx.Message);
            return StatusCode(500, OperationResult.Failed(pmEx.Message, pmEx.StepCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting payment method {PaymentMethodId}", paymentMethodId);
            return StatusCode(500, OperationResult.Failed("An unexpected server error occurred.", "UNHANDLED_EXCEPTION"));
        }
    }

    // POST: api/paymentmethod/user/{userId}/setdefault/{paymentMethodId}
    [HttpPost("user/{userId}/setdefault/{paymentMethodId}")]
    public async Task<ActionResult<OperationResult>> SetDefaultPaymentMethod(Guid userId, Guid paymentMethodId)
    {
         try
        {
            var result = await _paymentMethodService.SetDefaultPaymentMethodAsync(userId, paymentMethodId);
            if (!result.Success)
            {
                if (result.ErrorCode == "PM_NOT_FOUND") return NotFound(result);
                return BadRequest(result);
            }
            return Ok(result);
        }
        catch (PaymentMethodStepException pmEx)
        {
            _logger.LogWarning(pmEx, "PaymentMethodStepException: {ErrorCode} - {Message}", pmEx.StepCode, pmEx.Message);
            return StatusCode(500, OperationResult.Failed(pmEx.Message, pmEx.StepCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error setting default payment method for user {UserId}", userId);
            return StatusCode(500, OperationResult.Failed("An unexpected server error occurred.", "UNHANDLED_EXCEPTION"));
        }
    }

    // GET: api/paymentmethod/types
    [HttpGet("types")]
    public async Task<ActionResult<List<PaymentMethodType>>> GetAvailablePaymentMethodTypes()
    {
        var types = await _paymentMethodService.GetAvailablePaymentMethodTypesAsync();
        return Ok(types);
    }
}