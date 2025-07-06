using Microsoft.AspNetCore.Mvc;
using Truckero.Core.Constants;
using Truckero.Core.DTOs.PayoutAccount;
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
            var responses = await _payoutAccountService.GetPayoutAccountsByUserIdAsync(userId);
            if (responses == null || responses.PayoutAccounts.Count == 0) {
                return NotFound(new List<PayoutAccountResponse>
                {
                    new PayoutAccountResponse
                    {
                        Success = false,
                        Message = "No payout accounts found for this user.",
                        ErrorCode = ExceptionCodes.PayoutAccountErrorCodes.NotFound,
                        PayoutAccounts = new List<PayoutAccountRequest>()
                    }
                });
            }
            return Ok(responses);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error retrieving payout accounts for user {UserId}", userId);
            return StatusCode(500, new List<PayoutAccountResponse>
            {
                new PayoutAccountResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred while retrieving payout accounts.",
                    ErrorCode = ExceptionCodes.UnhandledException,
                    PayoutAccounts = new List<PayoutAccountRequest>()
                }
            });
        }
    }

    // GET: api/payoutaccount/{payoutAccountId}
    [HttpGet("{payoutAccountId}")]
    public async Task<ActionResult<PayoutAccountResponse>> GetPayoutAccountById(Guid payoutAccountId) {
        try {
            var response = await _payoutAccountService.GetPayoutAccountByIdAsync(payoutAccountId);
            var account = response?.PayoutAccounts?.FirstOrDefault();
            if (response == null || account == null) {
                return NotFound(new PayoutAccountResponse {
                    Success = false,
                    Message = "Payout account not found.",
                    ErrorCode = ExceptionCodes.PayoutAccountErrorCodes.NotFound,
                    PayoutAccounts = new List<PayoutAccountRequest>()
                });
            }
            // Only return the found account in the collection
            return Ok(new PayoutAccountResponse {
                Success = response.Success,
                Message = response.Message,
                ErrorCode = response.ErrorCode,
                PayoutAccounts = new List<PayoutAccountRequest> { account }
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error retrieving payout account by ID {PayoutAccountId}", payoutAccountId);
            return StatusCode(500, new PayoutAccountResponse {
                Success = false,
                Message = "An unexpected error occurred while retrieving the payout account.",
                ErrorCode = ExceptionCodes.UnhandledException,
                PayoutAccounts = new List<PayoutAccountRequest>()
            });
        }
    }

    // GET: api/payoutaccount/user/{userId}/default
    [HttpGet("user/{userId}/default")]
    public async Task<ActionResult<PayoutAccountResponse>> GetDefaultPayoutAccountByUserId(Guid userId) {
        try {
            var response = await _payoutAccountService.GetDefaultPayoutAccountByUserIdAsync(userId);
            var account = response?.PayoutAccounts?.FirstOrDefault();
            if (response == null || account == null) {
                return Ok(new PayoutAccountResponse {
                    Success = false,
                    Message = "No default payout account.",
                    ErrorCode = null,
                    PayoutAccounts = new List<PayoutAccountRequest>()
                });
            }
            return Ok(new PayoutAccountResponse {
                Success = response.Success,
                Message = response.Message,
                ErrorCode = response.ErrorCode,
                PayoutAccounts = new List<PayoutAccountRequest> { account }
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error retrieving default payout account for user {UserId}", userId);
            return StatusCode(500, new PayoutAccountResponse {
                Success = false,
                Message = "An unexpected error occurred while retrieving the default payout account.",
                ErrorCode = ExceptionCodes.UnhandledException,
                PayoutAccounts = new List<PayoutAccountRequest>()
            });
        }
    }

    // POST: api/payoutaccount/user/{userId}
    [HttpPost("user/{userId}")]
    public async Task<ActionResult<PayoutAccountResponse>> AddPayoutAccount(Guid userId, [FromBody] PayoutAccountRequest request) {
        if (!ModelState.IsValid) {
            return BadRequest(new PayoutAccountResponse {
                Success = false,
                Message = "Invalid request data.",
                ErrorCode = ExceptionCodes.ValidationFailed,
                PayoutAccounts = new List<PayoutAccountRequest>()
            });
        }

        try {
            var response = await _payoutAccountService.AddPayoutAccountAsync(userId, request);
            var account = response?.PayoutAccounts?.FirstOrDefault();
            if (response == null || account == null) {
                return BadRequest(new PayoutAccountResponse {
                    Success = false,
                    Message = "Failed to create payout account.",
                    ErrorCode = ExceptionCodes.UnhandledException,
                    PayoutAccounts = new List<PayoutAccountRequest>()
                });
            }
            return CreatedAtAction(nameof(GetPayoutAccountById), new { payoutAccountId = account.Id }, response);
        } catch (PayoutAccountStepException paEx) {
            _logger.LogWarning(paEx, "PayoutAccountStepException while adding account for user {UserId}: {ErrorCode} - {ErrorMessage}", userId, paEx.StepCode, paEx.Message);
            return StatusCode(500, new PayoutAccountResponse {
                Success = false,
                Message = paEx.Message,
                ErrorCode = paEx.StepCode,
                PayoutAccounts = new List<PayoutAccountRequest>()
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error adding payout account for user {UserId}", userId);
            return StatusCode(500, new PayoutAccountResponse {
                Success = false,
                Message = "An unexpected server error occurred.",
                ErrorCode = ExceptionCodes.UnhandledException,
                PayoutAccounts = new List<PayoutAccountRequest>()
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
                ErrorCode = ExceptionCodes.ValidationFailed,
                PayoutAccounts = new List<PayoutAccountRequest>()
            });
        }

        try {
            var response = await _payoutAccountService.UpdatePayoutAccountAsync(userId, payoutAccountId, request);
            var account = response?.PayoutAccounts?.FirstOrDefault();
            if (response == null || account == null) {
                return NotFound(new PayoutAccountResponse {
                    Success = false,
                    Message = "Payout account not found.",
                    ErrorCode = ExceptionCodes.PayoutAccountErrorCodes.NotFound,
                    PayoutAccounts = new List<PayoutAccountRequest>()
                });
            }
            return Ok(response);
        } catch (PayoutAccountStepException paEx) {
            _logger.LogWarning(paEx, "PayoutAccountStepException while updating account {PayoutAccountId} for user {UserId}: {ErrorCode} - {ErrorMessage}", payoutAccountId, userId, paEx.StepCode, paEx.Message);
            return StatusCode(500, new PayoutAccountResponse {
                Success = false,
                Message = paEx.Message,
                ErrorCode = paEx.StepCode,
                PayoutAccounts = new List<PayoutAccountRequest>()
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error updating payout account {PayoutAccountId} for user {UserId}", payoutAccountId, userId);
            return StatusCode(500, new PayoutAccountResponse {
                Success = false,
                Message = "An unexpected server error occurred.",
                ErrorCode = ExceptionCodes.UnhandledException,
                PayoutAccounts = new List<PayoutAccountRequest>()
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
            return NotFound(new { Success = false, Message = "Payout account not found.", ErrorCode = ExceptionCodes.PayoutAccountErrorCodes.NotFound });
        } catch (PayoutAccountOperationException ex) when (ex.StepCode == ExceptionCodes.PayoutAccountErrorCodes.CannotDeleteDefault) {
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
            return NotFound(new { Success = false, Message = "Payout account not found.", ErrorCode = ExceptionCodes.PayoutAccountErrorCodes.NotFound });
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error setting default payout account {PayoutAccountId} for user {UserId}", payoutAccountId, userId);
            return StatusCode(500, new { Success = false, Message = "An unexpected server error occurred.", ErrorCode = ExceptionCodes.UnhandledException });
        }
    }
}
