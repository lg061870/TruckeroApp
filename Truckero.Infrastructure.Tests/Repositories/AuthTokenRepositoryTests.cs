// 📦 Repository Unit Tests: AuthToken + User Repositories

using Truckero.Core.Entities;
using Truckero.Diagnostics.Data;
using Truckero.Infrastructure.Repositories;
using Xunit;

namespace Truckero.Infrastructure.Tests.Repositories
{
    public class AuthTokenRepositoryTests : TestBase
    {
        private readonly AuthTokenRepository _sut;
        
        public AuthTokenRepositoryTests() : base()
        {
            _sut = new AuthTokenRepository(_dbContext);
            
            // Seed test data asynchronously but wait for completion
            SeedTestDatabaseAsync().GetAwaiter().GetResult();
        }
        
        [Fact]
        public async Task AddTokenAsync_WithExistingUser_ShouldPersistToken()
        {
            // Arrange - Use a known user from mock data
            var userId = MockUserTestData.Ids.CustomerUserId;
            await EnsureUserExistsAsync(userId);
            
            var tokenKey = "new-test-refresh-token";
            var accessKey = "new-test-access-token";
            var token = new AuthToken
            {
                UserId = userId,
                RefreshToken = tokenKey,
                AccessToken = accessKey,
                IssuedAt = System.DateTime.UtcNow,
                ExpiresAt = System.DateTime.UtcNow.AddDays(7)
            };
            
            // Act
            await _sut.AddTokenAsync(token);
            
            // Assert - Verify token was saved
            var result = await _sut.GetByRefreshTokenByRefreshTokenKeyAsync(tokenKey);
            Assert.NotNull(result);
            Assert.Equal(tokenKey, result!.RefreshToken);
            Assert.Equal(accessKey, result.AccessToken);
            Assert.Equal(userId, result.UserId);
        }
        
        [Fact]
        public async Task GetAccessTokenByAccessTokenKeyAsync_WithValidToken_ShouldReturnToken()
        {
            // Arrange - Use token from mock data
            var accessToken = MockAuthTokenTestData.Tokens.ValidCustomerAccess;
            
            // Act
            var result = await _sut.GetAccessTokenByAccessTokenKeyAsync(accessToken);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(accessToken, result!.AccessToken);
            Assert.Equal(MockUserTestData.Ids.CustomerUserId, result.UserId);
        }
        
        [Fact]
        public async Task DeleteAllTokensForUserAsync_ShouldRemoveAllUserTokens()
        {
            // Arrange - Ensure user exists and has tokens
            var userId = MockUserTestData.Ids.CustomerUserId;
            await EnsureUserExistsAsync(userId);
            
            // Add a new token for this test
            var newToken = new AuthToken
            {
                UserId = userId,
                RefreshToken = "delete-test-refresh",
                AccessToken = "delete-test-access",
                IssuedAt = System.DateTime.UtcNow,
                ExpiresAt = System.DateTime.UtcNow.AddDays(7)
            };
            
            await _sut.AddTokenAsync(newToken);
            
            // Act
            await _sut.DeleteAllTokensForUserAsync(userId);
            
            // Assert
            var result = await _sut.GetByRefreshTokenByRefreshTokenKeyAsync("delete-test-refresh");
            Assert.Null(result);
            
            // Also check that the mock token was deleted
            var mockResult = await _sut.GetByRefreshTokenByRefreshTokenKeyAsync(
                MockAuthTokenTestData.Tokens.ValidCustomerRefresh);
            Assert.Null(mockResult);
        }
    }
}
