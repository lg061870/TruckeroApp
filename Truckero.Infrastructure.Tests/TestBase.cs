using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Diagnostics;
using Truckero.Diagnostics.Data;
using Truckero.Infrastructure.Data;
using Xunit;

namespace Truckero.Infrastructure.Tests
{
    public abstract class TestBase : IDisposable
    {
        protected readonly AppDbContext _dbContext;
        
        protected TestBase()
        {
            // Get DbContext from provider (uses cloned database)
            _dbContext = TestDbContextProvider.CreateDbContext<AppDbContext>();
        }
        
        public void Dispose()
        {
            _dbContext?.Dispose();
        }
        
        /// <summary>
        /// Seeds the test database with all required mock data
        /// </summary>
        protected async Task SeedTestDatabaseAsync()
        {
            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedAuthTokensAsync();
            await SeedPaymentMethodTypesAsync();
            // Add additional seeding as needed
        }
        
        /// <summary>
        /// Seeds required role data if not already present
        /// </summary>
        protected async Task SeedRolesAsync()
        {
            var roles = MockRoleTestData.GetAllRoles();
            foreach (var role in roles)
            {
                var existingRole = await _dbContext.Roles.FindAsync(role.Id);
                if (existingRole == null)
                {
                    _dbContext.Roles.Add(role);
                }
            }
            await _dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// Seeds required user data if not already present
        /// </summary>
        protected async Task SeedUsersAsync() {
            var users = new List<User>
            {
                MockUserTestData.CustomerUser,
                MockUserTestData.DriverUser,
                MockUserTestData.AdminUser
            };

            foreach (var user in users) {
                var existingUser = await _dbContext.Users.FindAsync(user.Id);
                if (existingUser == null) {
                    _dbContext.Users.Add(user);
                }
            }
            await _dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// Seeds required auth token data if not already present
        /// </summary>
        protected async Task SeedAuthTokensAsync()
        {
            var tokens = MockAuthTokenTestData.GetAllTestTokens();
            foreach (var token in tokens)
            {
                var existingToken = await _dbContext.AuthTokens
                    .FirstOrDefaultAsync(t => t.AccessToken == token.AccessToken);
                    
                if (existingToken == null)
                {
                    _dbContext.AuthTokens.Add(token);
                }
            }
            await _dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// Seeds required payment method types if not already present
        /// </summary>
        protected async Task SeedPaymentMethodTypesAsync()
        {
            var cardType = MockCustomerTestData.CardPaymentMethodType;
            var bankType = MockCustomerTestData.BankPaymentMethodType;
            
            var existingCardType = await _dbContext.PaymentMethodTypes.FindAsync(cardType.Id);
            var existingBankType = await _dbContext.PaymentMethodTypes.FindAsync(bankType.Id);
            
            if (existingCardType == null)
                _dbContext.PaymentMethodTypes.Add(cardType);
                
            if (existingBankType == null)
                _dbContext.PaymentMethodTypes.Add(bankType);
                
            await _dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// Gets an existing user or creates it if needed
        /// </summary>
        protected async Task<User> EnsureUserExistsAsync(Guid userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
                return user;
                
            // Try to find in mock data
            user = MockUserTestData.GetAllTestUsers().FirstOrDefault(u => u.Id == userId);
            
            if (user == null)
                throw new ArgumentException($"No mock user found with ID: {userId}");
                
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }
    }

    public static class AppDbContextExtensions
    {
        public static async Task<User> EnsureUserExistsAsync(this AppDbContext dbContext, User user)
        {
            // Make sure the user has a valid RoleId
            if (user.RoleId == Guid.Empty)
            {
                // Use MockRoleTestData instead of hardcoded role ID
                user.RoleId = MockRoleTestData.Ids.CustomerRoleId;
            }
            
            // Add the user
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();
            
            // Create an OnboardingProgress if it doesn't exist
            var onboarding = await dbContext.Onboardings.FirstOrDefaultAsync(o => o.UserId == user.Id);
            if (onboarding == null)
            {
                onboarding = new OnboardingProgress
                {
                    UserId = user.Id,
                    StepCurrent = "start",
                    Completed = false,
                    LastUpdated = DateTime.UtcNow
                };
                
                dbContext.Onboardings.Add(onboarding);
                await dbContext.SaveChangesAsync();
            }
            
            // Create an AuthToken if the user doesn't have one
            if (!await dbContext.AuthTokens.AnyAsync(t => t.UserId == user.Id))
            {
                var authToken = new AuthToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    AccessToken = Guid.NewGuid().ToString(),
                    RefreshToken = Guid.NewGuid().ToString(),
                    IssuedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(1)
                };
                dbContext.AuthTokens.Add(authToken);
                await dbContext.SaveChangesAsync();
            }
            
            return user;
        }
    }
}