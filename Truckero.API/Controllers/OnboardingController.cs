using Microsoft.AspNetCore.Mvc;
using Truckero.Core.DTOs.Auth;
using Truckero.Core.DTOs.Onboarding;
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

    /// <summary>
    /// Completes onboarding for a customer user.
    /// </summary>
    [HttpPost("customer")]
    public async Task<IActionResult> CompleteCustomerOnboarding(
        [FromQuery] Guid userId,
        [FromBody] CustomerProfileRequest request)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { message = "Missing or invalid userId" });

        await _onboardingService.CompleteCustomerOnboardingAsync(request, userId);
        return Ok(new { message = "Customer onboarding complete" });
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
            return BadRequest(new { message = "Missing or invalid userId" });

        try
        {
            await _onboardingService.CompleteDriverOnboardingAsync(request, userId);
            return Ok(new { message = "Driver onboarding complete" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing driver onboarding for userId {UserId}", userId);
            return StatusCode(500, new { message = "Unexpected server error during onboarding." });
        }
    }
}
