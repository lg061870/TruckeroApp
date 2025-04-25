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

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartOnboardingRequest request)
    {
        var userId = GetUserId(); // TODO: Replace with real identity extraction
        await _onboardingService.StartAsync(request, userId);
        return Accepted(new { message = "Verification sent" });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyCodeRequest request)
    {
        var userId = GetUserId(); // TODO: Replace with real identity extraction
        var result = await _onboardingService.VerifyCodeAsync(request, userId);

        if (!result)
            return Unauthorized(new { message = "Invalid verification code" });

        return Ok(new { message = "Phone/email verified" });
    }

    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress()
    {
        var userId = GetUserId(); // TODO: Replace with real identity extraction
        var progress = await _onboardingService.GetProgressAsync(userId);
        return Ok(progress);
    }

    // 🚧 Stub for extracting the current user id
    private Guid GetUserId()
    {
        // In real implementation, parse from ClaimsPrincipal (JWT)
        return Guid.Parse("00000000-0000-0000-0000-000000000001");
    }
}
