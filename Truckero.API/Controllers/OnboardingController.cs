using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Truckero.Core.DTOs.Common;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Services;

namespace Truckero.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("onboarding")]
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingService _onboardingService;
    private readonly ILogger<OnboardingController> _logger;

    public OnboardingController(IOnboardingService onboardingService, ILogger<OnboardingController> logger)
    {
        _onboardingService = onboardingService;
        _logger = logger;
    }

    /// <summary>
    /// Starts the onboarding process (e.g. sends verification code).
    /// </summary>
    [AllowAnonymous] // Registration start should be open
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromQuery] Guid userId, [FromBody] StartOnboardingRequest request)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { message = "Missing or invalid userId" });

        await _onboardingService.StartAsync(request, userId);
        return Accepted(new { message = "Verification sent" });
    }

    /// <summary>
    /// Verifies submitted code (e.g. via SMS or email).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyCodeRequest request)
    {
        if (request.UserId == Guid.Empty)
            return BadRequest(new { message = "Missing or invalid userId" });

        var result = await _onboardingService.VerifyCodeAsync(request, request.UserId);

        if (!result)
            return Unauthorized(new { message = "Invalid verification code" });

        return Ok(new { message = "Phone/email verified" });
    }

    /// <summary>
    /// Returns the user's current onboarding progress.
    /// </summary>
    [Authorize] // Progress is sensitive; lock down
    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress([FromQuery] Guid userId)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { message = "Missing or invalid userId" });

        var progress = await _onboardingService.GetProgressAsync(userId);
        return Ok(progress);
    }

    // TODO [Security Upgrade – Phase 2]:
    // Replace [AllowAnonymous] with custom [AppAuthorize] attribute.
    // This endpoint will eventually accept JWTs signed by the Truckero mobile app.
    // The token is generated client-side and validated server-side using RS256 public key cryptography.
    // Expected Claims: { "scope": "app", "client_id": "truckero-mobile" }
    // Validation: Use shared public key (JWKS), check audience and scope.
    // NOTE: This is a temporary auth bypass until full JWT-based access control is enforced platform-wide.
    //[Authorize] // Onboarding completion should require authentication
    [AllowAnonymous] // For now, allow unauthenticated access to onboarding completion
    [HttpPost("customer")]
    public async Task<IActionResult> CompleteCustomerOnboarding([FromBody] CustomerOnboardingRequest request)
    {
        try
        {
            var token = await _onboardingService.CompleteCustomerOnboardingAsync(request);
            return Ok(token);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { error = ex.Message, code = "duplicate_email" });
        }
        catch (OnboardingStepException ex)
        {
            return BadRequest(new { error = ex.Message, code = ex.StepCode });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message, code = "invalid_operation" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during customer onboarding");
            return StatusCode(500, new { error = "An unexpected error occurred during registration." });
        }
    }

    /// <summary>
    /// Completes onboarding for a driver user.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("driver")]
    public async Task<IActionResult> CompleteDriverOnboarding(
        [FromQuery] Guid userId,
        [FromBody] DriverProfileRequest request)
    {
        try
        {
            var token = await _onboardingService.CompleteDriverOnboardingAsync(request);
            return Ok(token);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { error = ex.Message, code = "duplicate_email" });
        }
        catch (OnboardingStepException ex)
        {
            return BadRequest(new { error = ex.Message, code = ex.StepCode });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message, code = "invalid_operation" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during driver onboarding");
            return StatusCode(500, new { error = "An unexpected error occurred during registration." });
        }

    }

    /// <summary>
    /// Sends a new confirmation email to the user.
    /// </summary>
    [AllowAnonymous] // Email confirmation is usually open (user may be logged out!)
    [HttpPost("send-confirmation-email")]
    public async Task<IActionResult> SendConfirmationEmail([FromQuery] Guid userId)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { message = "Missing or invalid userId" });

        var result = await _onboardingService.SendConfirmationEmailAsync(userId);
        if (result.Success)
            return Ok(new OperationResult { Success = true,  Message = result.Message });
        else
            return BadRequest(new { error = result.Message });
    }

    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { error = "Missing or invalid token." });

        try
        {
            var result = await _onboardingService.ConfirmEmailAsync(token);
            if (result.Success)
                return Ok(new { message = "Email confirmed successfully." });
            else
                return BadRequest(new { error = result.Message ?? "Invalid or expired confirmation token." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email with token {Token}", token);
            return StatusCode(500, new { error = "An unexpected error occurred during email confirmation." });
        }
    }
}
