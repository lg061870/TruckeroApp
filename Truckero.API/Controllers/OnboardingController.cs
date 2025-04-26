using Microsoft.AspNetCore.Mvc;
using Truckero.Core.DTOs.Onboarding;
using Truckero.Core.Interfaces;

namespace Truckero.API.Controllers;

[ApiController]
[Route("onboarding")]
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingService _onboardingService;

    public OnboardingController(IOnboardingService onboardingService)
    {
        _onboardingService = onboardingService;
    }

    /// <summary>
    /// Starts the onboarding process for a user (e.g., send verification).
    /// </summary>
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromQuery] Guid userId, [FromBody] StartOnboardingRequest request)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { message = "Missing or invalid userId" });

        if (string.IsNullOrWhiteSpace(request.Phone))
            return BadRequest(new { message = "Phone number is required" });

        await _onboardingService.StartAsync(request, userId);
        return Accepted(new { message = "Verification sent" });
    }

    /// <summary>
    /// Verifies a code submitted by the user (SMS/email).
    /// </summary>
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyCodeRequest request)
    {
        if (request.UserId == Guid.Empty)
            return BadRequest(new { message = "Missing or invalid userId" });

        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(new { message = "Verification code is required" });

        var result = await _onboardingService.VerifyCodeAsync(request, request.UserId);

        if (!result)
            return Unauthorized(new { message = "Invalid verification code" });

        return Ok(new { message = "Phone/email verified" });
    }


    /// <summary>
    /// Returns current onboarding status for the user.
    /// </summary>
    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress([FromQuery] Guid userId)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { message = "Missing or invalid userId" });

        var progress = await _onboardingService.GetProgressAsync(userId);
        return Ok(progress);
    }
}
