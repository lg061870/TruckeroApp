using Microsoft.AspNetCore.Mvc;
using Truckero.Core.DTOs.PaymentAccount;
using Truckero.Core.Interfaces.Services;
using Truckero.Core.Constants; // For error codes
using Microsoft.Extensions.Logging;

namespace Truckero.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentAccountController : ControllerBase {
    private readonly IPaymentAccountService _paymentAccountService;
    private readonly ILogger<PaymentAccountController> _logger;

    public PaymentAccountController(
        IPaymentAccountService paymentAccountService,
        ILogger<PaymentAccountController> logger) {
        _paymentAccountService = paymentAccountService ?? throw new ArgumentNullException(nameof(paymentAccountService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // GET: api/paymentaccount/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<PaymentAccountResponse>> GetPaymentAccountsByUserId(Guid userId) {
        try {
            var response = await _paymentAccountService.GetPaymentAccountsByUserIdAsync(userId);
            return Ok(response);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error retrieving payment accounts for user {UserId}", userId);
            return StatusCode(500, new PaymentAccountResponse {
                Success = false,
                Message = "An unexpected error occurred while retrieving payment accounts.",
                ErrorCode = ExceptionCodes.UnhandledException
            });
        }
    }

    // GET: api/paymentaccount/{paymentAccountId}
    [HttpGet("{paymentAccountId}")]
    public async Task<ActionResult<PaymentAccountResponse>> GetPaymentAccountById(Guid paymentAccountId) {
        try {
            var response = await _paymentAccountService.GetPaymentAccountByIdAsync(paymentAccountId);
            if (response == null || response.PaymentAccounts.Count == 0) {
                return NotFound(new PaymentAccountResponse {
                    Success = false,
                    Message = "Payment account not found.",
                    ErrorCode = ExceptionCodes.PayoutAccountErrorCodes.NotFound
                });
            }
            return Ok(response);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error retrieving payment account by ID {PaymentAccountId}", paymentAccountId);
            return StatusCode(500, new PaymentAccountResponse {
                Success = false,
                Message = "An unexpected error occurred while retrieving the payment account.",
                ErrorCode = ExceptionCodes.UnhandledException
            });
        }
    }

    // POST: api/paymentaccount
    [HttpPost]
    public async Task<ActionResult<PaymentAccountResponse>> AddPaymentAccount([FromBody] PaymentAccountRequest request) {
        if (!ModelState.IsValid) {
            return BadRequest(new PaymentAccountResponse {
                Success = false,
                Message = "Invalid request data.",
                ErrorCode = ExceptionCodes.ValidationFailed
            });
        }

        try {
            var response = await _paymentAccountService.AddPaymentAccountAsync(request);
            if (response == null || response.PaymentAccounts.Count == 0) {
                return BadRequest(new PaymentAccountResponse {
                    Success = false,
                    Message = "Failed to create payment account.",
                    ErrorCode = ExceptionCodes.UnhandledException
                });
            }

            return CreatedAtAction(nameof(GetPaymentAccountById),
                new { paymentAccountId = response.PaymentAccounts[0].Id }, response);
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error adding payment account");
            return StatusCode(500, new PaymentAccountResponse {
                Success = false,
                Message = "An unexpected server error occurred.",
                ErrorCode = ExceptionCodes.UnhandledException
            });
        }
    }

    // PUT: api/paymentaccount
    [HttpPut]
    public async Task<ActionResult<PaymentAccountResponse>> UpdatePaymentAccount([FromBody] PaymentAccountRequest request) {
        if (!ModelState.IsValid) {
            return BadRequest(new PaymentAccountResponse {
                Success = false,
                Message = "Invalid request data.",
                ErrorCode = ExceptionCodes.ValidationFailed
            });
        }

        try {
            var response = await _paymentAccountService.UpdatePaymentAccountAsync(request);
            if (response == null || response.PaymentAccounts.Count == 0) {
                return NotFound(new PaymentAccountResponse {
                    Success = false,
                    Message = "Payment account not found.",
                    ErrorCode = ExceptionCodes.PayoutAccountErrorCodes.NotFound
                });
            }

            return Ok(response);
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error updating payment account {PaymentAccountId}", request.Id);
            return StatusCode(500, new PaymentAccountResponse {
                Success = false,
                Message = "An unexpected server error occurred.",
                ErrorCode = ExceptionCodes.UnhandledException
            });
        }
    }

    // DELETE: api/paymentaccount/user/{userId}/{paymentAccountId}
    [HttpDelete("user/{userId}/{paymentAccountId}")]
    public async Task<ActionResult<PaymentAccountResponse>> DeletePaymentAccount(Guid userId, Guid paymentAccountId) {
        try {
            var response = await _paymentAccountService.DeletePaymentAccountAsync(userId, paymentAccountId);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error deleting payment account {PaymentAccountId} for user {UserId}", paymentAccountId, userId);
            return StatusCode(500, new PaymentAccountResponse {
                Success = false,
                Message = "An unexpected server error occurred.",
                ErrorCode = ExceptionCodes.UnhandledException
            });
        }
    }

    // POST: api/paymentaccount/user/{userId}/setdefault/{paymentAccountId}
    [HttpPost("user/{userId}/setdefault/{paymentAccountId}")]
    public async Task<ActionResult<PaymentAccountResponse>> SetDefaultPaymentAccount(Guid userId, Guid paymentAccountId) {
        try {
            var response = await _paymentAccountService.SetDefaultPaymentAccountAsync(userId, paymentAccountId);
            return Ok(response);
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error setting default payment account {PaymentAccountId} for user {UserId}", paymentAccountId, userId);
            return StatusCode(500, new PaymentAccountResponse {
                Success = false,
                Message = "An unexpected server error occurred.",
                ErrorCode = ExceptionCodes.UnhandledException
            });
        }
    }

    // POST: api/paymentaccount/user/{userId}/validate/{paymentAccountId}
    [HttpPost("user/{userId}/validate/{paymentAccountId}")]
    public async Task<ActionResult<PaymentAccountResponse>> MarkPaymentAccountValidated(Guid userId, Guid paymentAccountId) {
        try {
            var response = await _paymentAccountService.MarkPaymentAccountValidatedAsync(userId, paymentAccountId);
            return Ok(response);
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error validating payment account {PaymentAccountId} for user {UserId}", paymentAccountId, userId);
            return StatusCode(500, new PaymentAccountResponse {
                Success = false,
                Message = "An unexpected server error occurred.",
                ErrorCode = ExceptionCodes.UnhandledException
            });
        }
    }
}
