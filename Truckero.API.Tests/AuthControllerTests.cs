using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Truckero.Core.DTOs.Auth;
using Xunit;

namespace Truckero.API.Tests;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<ProgramEntry>>
{
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactory<ProgramEntry> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("UnitTesting");
        }).CreateClient();
    }

    #region REGISTER Tests

    // 🔐 REGISTER: Valid user info should return 201 Created
    [Fact]
    public async Task Register_Should_Return_201()
    {
        var payload = new RegisterUserRequest
        {
            Email = "testuser@example.com",
            Password = "StrongPass123!",
            Role = "Customer",
            Name = "Test User"
        };

        var response = await _client.PostAsJsonAsync("/auth/register", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // 🔐 REGISTER: Missing fields should return 400 BadRequest
    [Fact]
    public async Task Register_Should_Return_400_On_Invalid_Input()
    {
        var request = new RegisterUserRequest
        {
            Email = "",
            Password = "123",
            Role = "",
            Name = ""
        };

        var response = await _client.PostAsJsonAsync("/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region LOGIN Tests

    // 🔑 LOGIN: Valid credentials should return 200 OK
    [Fact]
    public async Task Login_Should_Return_200_On_Valid_Credentials()
    {
        var request = new AuthLoginRequest
        {
            Email = "testuser@example.com",
            Password = "StrongPass123!"
        };

        var response = await _client.PostAsJsonAsync("/auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrEmpty(content?.AccessToken));
        Assert.False(string.IsNullOrEmpty(content?.RefreshToken));
    }

    // 🔑 LOGIN: Invalid credentials should return 401 Unauthorized
    [Fact]
    public async Task Login_Should_Return_401_On_Invalid_Credentials()
    {
        var request = new AuthLoginRequest
        {
            Email = "invalid@example.com",
            Password = "wrongpass"
        };

        var response = await _client.PostAsJsonAsync("/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region REFRESH Token Tests

    // 🔄 REFRESH: Valid refresh token should return new tokens
    [Fact]
    public async Task Refresh_Should_Return_200_On_Valid_RefreshToken()
    {
        var request = new RefreshTokenRequest
        {
            RefreshToken = "valid.refresh.token"
        };

        var response = await _client.PostAsJsonAsync("/auth/refresh", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrEmpty(content?.AccessToken));
        Assert.False(string.IsNullOrEmpty(content?.RefreshToken));
    }

    // 🔄 REFRESH: Empty token should return 400 BadRequest
    [Fact]
    public async Task Refresh_Should_Return_400_On_Empty_Token()
    {
        var request = new RefreshTokenRequest
        {
            RefreshToken = ""
        };

        var response = await _client.PostAsJsonAsync("/auth/refresh", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // TODO: Activate once real refresh token validation logic is implemented
    //// 🔄 REFRESH: Invalid refresh token placeholder (deferred)
    //[Fact]
    //public async Task Refresh_Should_Return_400_On_Invalid_RefreshToken()
    //{
    //    var request = new RefreshTokenRequest
    //    {
    //        RefreshToken = "invalid.refresh.token"
    //    };

    //    var response = await _client.PostAsJsonAsync("/auth/refresh", request);

    //    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    //}

    #endregion

    #region LOGOUT Tests

    // 🚪 LOGOUT: Valid userId should return 204 NoContent
    [Fact]
    public async Task Logout_Should_Return_204()
    {
        var userId = Guid.NewGuid();

        var response = await _client.PostAsync($"/auth/logout?userId={userId}", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // 🚪 LOGOUT: Invalid userId should still return 204 NoContent
    [Fact]
    public async Task Logout_Should_Still_Return_204_On_Invalid_UserId()
    {
        var invalidUserId = Guid.NewGuid();

        var response = await _client.PostAsync($"/auth/logout?userId={invalidUserId}", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion

    #region PASSWORD RESET Request Tests

    // 🔒 PASSWORD RESET REQUEST: Valid email should return 202 Accepted
    [Fact]
    public async Task RequestPasswordReset_Should_Return_202()
    {
        var request = new PasswordResetRequest
        {
            Email = "user@example.com"
        };

        var response = await _client.PostAsJsonAsync("/auth/password-reset/request", request);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    // 🔒 PASSWORD RESET REQUEST: Empty email should return 400 BadRequest
    [Fact]
    public async Task RequestPasswordReset_Should_Return_400_On_Empty_Email()
    {
        var request = new PasswordResetRequest
        {
            Email = ""
        };

        var response = await _client.PostAsJsonAsync("/auth/password-reset/request", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region PASSWORD RESET Confirm Tests

    // 🔐 CONFIRM RESET: Valid token and password should return 200 OK
    [Fact]
    public async Task ConfirmPasswordReset_Should_Return_200()
    {
        var request = new PasswordResetConfirmRequest
        {
            Token = "valid-reset-token",
            NewPassword = "NewSecurePass123!"
        };

        var response = await _client.PostAsJsonAsync("/auth/password-reset/confirm", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 🔐 CONFIRM RESET: Weak password should return 400 BadRequest
    [Fact]
    public async Task ConfirmPasswordReset_Should_Return_400_On_TooShort_Password()
    {
        var request = new PasswordResetConfirmRequest
        {
            Token = "valid-token",
            NewPassword = "123"
        };

        var response = await _client.PostAsJsonAsync("/auth/password-reset/confirm", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 🔐 CONFIRM RESET: Missing token should return 400 BadRequest
    [Fact]
    public async Task ConfirmPasswordReset_Should_Return_400_On_Missing_Token()
    {
        var request = new PasswordResetConfirmRequest
        {
            Token = "",
            NewPassword = "StrongPass123!"
        };

        var response = await _client.PostAsJsonAsync("/auth/password-reset/confirm", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}
