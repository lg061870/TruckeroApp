using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Truckero.Core.Constants;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Services;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PayoutAccountController : ControllerBase {
    private readonly IPayoutAccountService _payoutAccountService;
    private readonly ILogger<PayoutAccountController> _logger;

    public PayoutAccountController(IPayoutAccountService payoutAccountService, ILogger<PayoutAccountController> logger) {
        _payoutAccountService = payoutAccountService ?? throw new ArgumentNullException(nameof(payoutAccountService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // GET: api/payoutaccount/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<PayoutAccountResponse>>> GetPayoutAccountsByUserId(Guid userId) {
        try {
            var accounts = await _payoutAccountService.GetPayoutAccountsByUserIdAsync(userId);
            return Ok(accounts);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error retrieving payout accounts for user {UserId}", userId);
            return StatusCode(500, "An unexpected error occurred while retrieving payout accounts.");
        }
    }

    // GET: api/payoutaccount/{payoutAccountId}
    [HttpGet("{payoutAccountId}")]
    public async Task<ActionResult<PayoutAccountResponse>> GetPayoutAccountById(Guid payoutAccountId) {
        try {
            var response = await _payoutAccountService.GetPayoutAccountByIdAsync(payoutAccountId);
            if (response == null || response.PayoutAccount == null) {
                return NotFound(new PayoutAccountResponse {
                    Success = false,
                    Message = "Payout account not found.",
                    ErrorCode = ExceptionCodes.PayoutAccountNotFound
                });
            }
            return Ok(response);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error retrieving payout account by ID {PayoutAccountId}", payoutAccountId);
            return StatusCode(500, "An unexpected error occurred while retrieving the payout account.");
        }
    }

    // GET: api/payoutaccount/user/{userId}/default
    [HttpGet("user/{userId}/default")]
    public async Task<ActionResult<PayoutAccountResponse>> GetDefaultPayoutAccountByUserId(Guid userId) {
        try {
            var response = await _payoutAccountService.GetDefaultPayoutAccountByUserIdAsync(userId);
            if (response == null || response.PayoutAccount == null) {
                return Ok(new PayoutAccountResponse { Success = false, Message = "No default payout account.", ErrorCode = null, PayoutAccount = null });
            }
            return Ok(response);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error retrieving default payout account for user {UserId}", userId);
            return StatusCode(500, "An unexpected error occurred while retrieving the default payout account.");
        }
    }

    // POST: api/payoutaccount/user/{userId}
    [HttpPost("user/{userId}")]
    public async Task<ActionResult<PayoutAccountResponse>> AddPayoutAccount(Guid userId, [FromBody] PayoutAccountRequest request) {
        if (!ModelState.IsValid) {
            return BadRequest(new PayoutAccountResponse {
                Success = false,
                Message = "Invalid request data.",
                ErrorCode = ExceptionCodes.ValidationError
            });
        }

        try {
            var response = await _payoutAccountService.AddPayoutAccountAsync(userId, request);
            if (response == null || response.PayoutAccount == null) {
                return BadRequest(new PayoutAccountResponse {
                    Success = false,
                    Message = "Failed to create payout account.",
                    ErrorCode = ExceptionCodes.UnhandledException
                });
            }
            return CreatedAtAction(nameof(GetPayoutAccountById), new { payoutAccountId = response.PayoutAccount.Id }, response);
        } catch (PayoutAccountStepException paEx) {
            _logger.LogWarning(paEx, "PayoutAccountStepException while adding account for user {UserId}: {ErrorCode} - {ErrorMessage}", userId, paEx.StepCode, paEx.Message);
            return StatusCode(500, new PayoutAccountResponse {
                Success = false,
                Message = paEx.Message,
                ErrorCode = paEx.StepCode
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error adding payout account for user {UserId}", userId);
            return StatusCode(500, new PayoutAccountResponse {
                Success = false,
                Message = "An unexpected server error occurred.",
                ErrorCode = ExceptionCodes.UnhandledException
            });
        }
    }

    // PUT: api/payoutaccount/user/{userId}/{payoutAccountId}
    [HttpPut("user/{userId}/{payoutAccountId}")]
    public async Task<ActionResult<PayoutAccountResponse>> UpdatePayoutAccount(Guid userId, Guid payoutAccountId, [FromBody] PayoutAccountRequest request) {
        if (!ModelState.IsValid) {
            return BadRequest(new PayoutAccountResponse {
                Success = false,
                Message = "Invalid request data.",
                ErrorCode = ExceptionCodes.ValidationError
            });
        }

        try {
            var response = await _payoutAccountService.UpdatePayoutAccountAsync(userId, payoutAccountId, request);
            if (response == null || response.PayoutAccount == null) {
                return NotFound(new PayoutAccountResponse {
                    Success = false,
                    Message = "Payout account not found.",
                    ErrorCode = ExceptionCodes.PayoutAccountNotFound
                });
            }
            return Ok(response);
        } catch (PayoutAccountStepException paEx) {
            _logger.LogWarning(paEx, "PayoutAccountStepException while updating account {PayoutAccountId} for user {UserId}: {ErrorCode} - {ErrorMessage}", payoutAccountId, userId, paEx.StepCode, paEx.Message);
            return StatusCode(500, new PayoutAccountResponse {
                Success = false,
                Message = paEx.Message,
                ErrorCode = paEx.StepCode
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error updating payout account {PayoutAccountId} for user {UserId}", payoutAccountId, userId);
            return StatusCode(500, new PayoutAccountResponse {
                Success = false,
                Message = "An unexpected server error occurred.",
                ErrorCode = ExceptionCodes.UnhandledException
            });
        }
    }

    // DELETE: api/payoutaccount/user/{userId}/{payoutAccountId}
    [HttpDelete("user/{userId}/{payoutAccountId}")]
    public async Task<ActionResult> DeletePayoutAccount(Guid userId, Guid payoutAccountId) {
        try {
            await _payoutAccountService.DeletePayoutAccountAsync(userId, payoutAccountId);
            return Ok(new { Success = true, Message = "Payout account deleted." });
        } catch (PayoutAccountNotFoundException) {
            return NotFound(new { Success = false, Message = "Payout account not found.", ErrorCode = ExceptionCodes.PayoutAccountNotFound });
        } catch (PayoutAccountOperationException ex) when (ex.StepCode == ExceptionCodes.CannotDeleteDefault) {
            return Conflict(new { Success = false, Message = ex.Message, ErrorCode = ex.StepCode });
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error deleting payout account {PayoutAccountId} for user {UserId}", payoutAccountId, userId);
            return StatusCode(500, new { Success = false, Message = "An unexpected server error occurred.", ErrorCode = ExceptionCodes.UnhandledException });
        }
    }

    // POST: api/payoutaccount/user/{userId}/setdefault/{payoutAccountId}
    [HttpPost("user/{userId}/setdefault/{payoutAccountId}")]
    public async Task<ActionResult> SetDefaultPayoutAccount(Guid userId, Guid payoutAccountId) {
        try {
            await _payoutAccountService.SetDefaultPayoutAccountAsync(userId, payoutAccountId);
            return Ok(new { Success = true, Message = "Default payout account set." });
        } catch (PayoutAccountNotFoundException) {
            return NotFound(new { Success = false, Message = "Payout account not found.", ErrorCode = ExceptionCodes.PayoutAccountNotFound });
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error setting default payout account {PayoutAccountId} for user {UserId}", payoutAccountId, userId);
            return StatusCode(500, new { Success = false, Message = "An unexpected server error occurred.", ErrorCode = ExceptionCodes.UnhandledException });
        }
    }

    // GET: api/payoutaccount/types
    [HttpGet("types")]
    public async Task<ActionResult<List<PaymentMethodType>>> GetAllPayoutPaymentMethods() {
        try {
            var types = await _payoutAccountService.GetAllPayoutPaymentMethodsAsync();
            return Ok(types);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error retrieving available payout method types.");
            return StatusCode(500, "An unexpected error occurred while retrieving payout method types.");
        }
    }

    // GET: api/payoutaccount/types/{countryCode}
    [HttpGet("types/{countryCode}")]
    public async Task<ActionResult<List<PaymentMethodType>>> GetAllPayoutPaymentMethodsByCountryCode(string countryCode) {
        try {
            var types = await _payoutAccountService.GetAllPayoutPaymentMethodsByCountryCodeAsync(countryCode);
            return Ok(types);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error retrieving payout method types for country {CountryCode}", countryCode);
            return StatusCode(500, "An unexpected error occurred while retrieving payout method types by country.");
        }
    }

    // GET: api/payoutaccount/reference/{countryCode}
    [HttpGet("reference/{countryCode}")]
    public async Task<ActionResult<PayoutPageReferenceDataDto>> GetPayoutPageReferenceData(string countryCode) {
        try {
            var data = await _payoutAccountService.GetPayoutPageReferenceDataAsync(countryCode);
            return Ok(data);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error retrieving payout page reference data for country {CountryCode}", countryCode);
            return StatusCode(500, "An unexpected error occurred while retrieving payout reference data.");
        }
    }
}
