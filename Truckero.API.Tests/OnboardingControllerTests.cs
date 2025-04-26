using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Truckero.Core.DTOs.Onboarding;
using Xunit;

namespace Truckero.API.Tests
{
    public class OnboardingControllerTests : IClassFixture<WebApplicationFactory<ProgramEntry>>
    {
        private readonly HttpClient _client;

        public OnboardingControllerTests(WebApplicationFactory<ProgramEntry> factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("UnitTesting");
            }).CreateClient();
        }

        // 🚚 START: Valid onboarding request
        [Fact]
        public async Task StartOnboarding_Should_Return_202_On_Valid_Input()
        {
            var userId = Guid.NewGuid();

            var request = new StartOnboardingRequest
            {
                Phone = "+15555551234",
                Role = "Driver"
            };

            var response = await _client.PostAsJsonAsync($"/onboarding/start?userId={userId}", request);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        // 🚫 START: Missing phone number
        [Fact]
        public async Task StartOnboarding_Should_Return_400_On_Missing_PhoneNumber()
        {
            var userId = Guid.NewGuid();

            var request = new StartOnboardingRequest
            {
                Phone = "",
                Role = "Driver"
            };

            var response = await _client.PostAsJsonAsync($"/onboarding/start?userId={userId}", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // 🚫 START: Missing user ID
        [Fact]
        public async Task StartOnboarding_Should_Return_400_On_Missing_UserId()
        {
            var request = new StartOnboardingRequest
            {
                Phone = "+15555551234",
                Role = "Driver"
            };

            var response = await _client.PostAsJsonAsync("/onboarding/start", request); // No userId in query string

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ✅ VERIFY: Valid verification
        [Fact]
        public async Task VerifyCode_Should_Return_200_On_Valid_Input()
        {
            var request = new VerifyCodeRequest
            {
                Code = "123456",
                UserId = Guid.NewGuid()
            };

            var response = await _client.PostAsJsonAsync("/onboarding/verify", request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // 🚫 VERIFY: Missing code — should trigger model validation
        [Fact]
        public async Task VerifyCode_Should_Return_400_On_Missing_Code()
        {
            var request = new VerifyCodeRequest
            {
                Code = "", // Invalid
                UserId = Guid.NewGuid(),
                Method = "sms"
            };

            var response = await _client.PostAsJsonAsync("/onboarding/verify", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // 🚫 VERIFY: Missing user ID — should trigger controller check
        [Fact]
        public async Task VerifyCode_Should_Return_400_On_Missing_UserId()
        {
            var request = new VerifyCodeRequest
            {
                Code = "123456",
                UserId = Guid.Empty, // Invalid
                Method = "sms"
            };

            var response = await _client.PostAsJsonAsync("/onboarding/verify", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // 🔎 PROGRESS: Should return 200 for valid user
        [Fact]
        public async Task GetProgress_Should_Return_200_For_Valid_User()
        {
            var userId = Guid.NewGuid();

            var response = await _client.GetAsync($"/onboarding/progress?userId={userId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ❗ PROGRESS: Should return 400 if userId missing
        [Fact]
        public async Task GetProgress_Should_Return_400_If_UserId_Missing()
        {
            var response = await _client.GetAsync("/onboarding/progress?userId=");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
