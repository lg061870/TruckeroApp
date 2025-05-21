using Microsoft.AspNetCore.Mvc;
using Truckero.Core.DTOs.Auth;
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
    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress([FromQuery] Guid userId)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { message = "Missing or invalid userId" });

        var progress = await _onboardingService.GetProgressAsync(userId);
        return Ok(progress);
    }

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
            return BadRequest(new { error = ex.Message, code = ex.Step });
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
    [HttpPost("driver")]
    public async Task<IActionResult> CompleteDriverOnboarding(
        [FromQuery] Guid userId,
        [FromBody] DriverProfileRequest request)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { error = "Missing or invalid userId" });

        try
        {
            var result = await _onboardingService.CompleteDriverOnboardingAsync(request, userId);

            if (result.Success)
            {
                return Ok(new { message = result.Message ?? "Driver onboarding complete" });
            }
            else
            {
                // Return a 400 Bad Request with the specific error message from the operation result
                return BadRequest(new { error = result.Message ?? "Failed to complete driver onboarding" });
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            // Return a 409 Conflict for duplicate driver
            return Conflict(new { error = ex.Message, code = "duplicate_driver" });
        }
        catch (InvalidOperationException ex)
        {
            // Return a 400 Bad Request for validation errors
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing driver onboarding for userId {UserId}", userId);
            return StatusCode(500, new { error = "An unexpected error occurred during driver registration." });
        }
    }
}
