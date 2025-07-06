using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Enums;
using Truckero.Diagnostics.Data;
using Truckero.Infrastructure.Repositories;
using Truckero.Infrastructure.Tests.Fixtures;
using Xunit;

namespace Truckero.Infrastructure.Tests.Repositories {
    public class UserRepositoryTests : IClassFixture<TestDbContextFixture> {
        private readonly TestDbContextFixture _fixture;
        private readonly UserRepository _repository;

        public UserRepositoryTests(TestDbContextFixture fixture) {
            _fixture = fixture;
            _repository = new UserRepository(fixture.DbContext);
        }

        // Helper method to ensure both role and auth token exist for a user
        private async Task<User> EnsureUserWithRoleAndTokenAsync(User user) {
            // Make sure the user has a valid RoleId
            if (user.RoleId == Guid.Empty) {
                user.RoleId = MockRoleTestData.Ids.CustomerRoleId;
            }

            // Check if user already exists
            var existingUser = await _repository.GetUserByIdAsync(user.Id);
            if (existingUser != null) {
                return existingUser; // User already exists with Role loaded
            }

            // Get the actual Role object from the database based on RoleId
            var role = await _fixture.DbContext.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId);
            if (role == null) {
                // Role doesn't exist in DB, let's use an existing role from the database
                // instead of trying to add one that might already be tracked
                role = await _fixture.DbContext.Roles.FirstOrDefaultAsync(r => r.Id == MockRoleTestData.Ids.CustomerRoleId);
                user.RoleId = role.Id;
            }

            // Assign the Role navigation property
            user.Role = role;

            // Create the user
            await _repository.AddUserAsync(user);
            await _repository.SaveUserChangesAsync();

            // Create an onboarding record if needed
            var onboarding = await _fixture.DbContext.Onboardings.FirstOrDefaultAsync(o => o.UserId == user.Id);
            if (onboarding == null) {
                onboarding = new OnboardingProgress {
                    UserId = user.Id,
                    StepCurrent = "start",
                    Completed = false,
                    LastUpdated = DateTime.UtcNow
                };
                _fixture.DbContext.Onboardings.Add(onboarding);
                await _fixture.DbContext.SaveChangesAsync();
            }

            // Create an auth token for the user if needed
            var existingToken = await _fixture.DbContext.AuthTokens
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (existingToken == null) {
                // Use predefined token if available, otherwise create a new one
                var token = MockUserTestData.GetAuthTokenForUser(user.Id);
                _fixture.DbContext.AuthTokens.Add(token);
                await _fixture.DbContext.SaveChangesAsync();
            }

            // Re-fetch the user to ensure all navigation properties are loaded
            return await _repository.GetUserByIdAsync(user.Id);
        }

        [Fact]
        public async Task AddAsync_And_Save_Should_Create_User() {
            // Arrange
            var role = await _fixture.DbContext.Roles.FirstAsync(r => r.Name == RoleType.Customer.ToString());

            var user = new User {
                Id = Guid.NewGuid(),
                Email = $"test_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com", // Use unique email
                FullName = "Test User",
                PasswordHash = "HASH_VALUE_HERE",
                PhoneNumber = "555-123-4567",
                Address = "123 Test St.",
                RoleId = role.Id,
                EmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await _repository.AddUserAsync(user);
            await _repository.SaveUserChangesAsync();

            // Also create an auth token for this user
            var token = new AuthToken {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                AccessToken = Guid.NewGuid().ToString(),
                RefreshToken = Guid.NewGuid().ToString(),
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            _fixture.DbContext.AuthTokens.Add(token);
            await _fixture.DbContext.SaveChangesAsync();

            // Assert
            var found = await _repository.GetUserByEmailAsync(user.Email);

            Assert.NotNull(found);
            Assert.Equal(user.Email, found!.Email);
            Assert.Equal(user.FullName, found.FullName);
            Assert.Equal(role.Id, found.RoleId);

            // Verify token was created
            var tokenExists = await _fixture.DbContext.AuthTokens.AnyAsync(t => t.UserId == user.Id);
            Assert.True(tokenExists);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllUsers() {
            // Arrange - Add some test users
            var testUser1 = MockUserTestData.DriverUser;
            var testUser2 = MockUserTestData.CustomerUser;

            // Ensure users exist using helper
            await EnsureUserWithRoleAndTokenAsync(testUser1);
            await EnsureUserWithRoleAndTokenAsync(testUser2);

            // Act
            var users = await _fixture.DbContext.Users.ToListAsync();

            // Assert
            Assert.NotNull(users);
            Assert.NotEmpty(users);
            Assert.Contains(users, u => u.Id == testUser1.Id);
            Assert.Contains(users, u => u.Id == testUser2.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsUser() {
            // Arrange
            var mockUser = MockUserTestData.CustomerUser;

            // Ensure user exists using helper
            await EnsureUserWithRoleAndTokenAsync(mockUser);

            // Act
            var user = await _repository.GetUserByIdAsync(mockUser.Id);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(mockUser.Id, user.Id);
            Assert.Equal(mockUser.Email, user.Email);
            Assert.Equal(mockUser.FullName, user.FullName);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull() {
            // Arrange
            var invalidUserId = Guid.NewGuid();

            // Act
            var user = await _repository.GetUserByIdAsync(invalidUserId);

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task AddAsync_AddsNewUser() {
            // Arrange - Create a new user with the correct properties
            var newUser = new User {
                Id = Guid.NewGuid(),
                Email = $"newuser_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com", // Ensure unique email
                FullName = "New Test User",
                PasswordHash = "hashedpassword123",
                PhoneNumber = "555-987-6543",
                Address = "456 New User St.",
                EmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RoleId = MockRoleTestData.Ids.CustomerRoleId // Use MockRoleTestData instead of hardcoded ID
            };

            // Act
            await EnsureUserWithRoleAndTokenAsync(newUser);
            var addedUser = await _repository.GetUserByIdAsync(newUser.Id);

            // Assert
            Assert.NotNull(addedUser);
            Assert.Equal(newUser.Email, addedUser.Email);
            Assert.Equal(newUser.FullName, addedUser.FullName);
            Assert.Equal(newUser.PhoneNumber, addedUser.PhoneNumber);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExistingUser() {
            // Arrange - Create a user with unique ID and email
            var user = new User {
                Id = Guid.NewGuid(),
                Email = $"update_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
                FullName = "User To Update",
                PasswordHash = "hashedpassword123",
                PhoneNumber = "555-111-2222",
                Address = "789 Update Ave.",
                EmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RoleId = MockRoleTestData.Ids.CustomerRoleId // Add the role ID
            };

            // Use the helper to ensure the user exists
            await EnsureUserWithRoleAndTokenAsync(user);

            // Verify user exists before updating
            var userBeforeUpdate = await _repository.GetUserByIdAsync(user.Id);
            Assert.NotNull(userBeforeUpdate);

            // Modify the user
            const string newFullName = "Updated User Name";
            const string newPhoneNumber = "555-333-4444";
            userBeforeUpdate.FullName = newFullName;
            userBeforeUpdate.PhoneNumber = newPhoneNumber;

            // Act
            await _fixture.DbContext.SaveChangesAsync();

            // Retrieve the updated user
            var updatedUser = await _repository.GetUserByIdAsync(user.Id);

            // Assert
            Assert.NotNull(updatedUser);
            Assert.Equal(newFullName, updatedUser.FullName);
            Assert.Equal(newPhoneNumber, updatedUser.PhoneNumber);
        }

        [Fact]
        public async Task DeleteAsync_RemovesUser() {
            // Arrange - Create a user we can safely delete
            var userToDelete = new User {
                Id = Guid.NewGuid(),
                Email = $"delete_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
                FullName = "User To Delete",
                PasswordHash = "hashedpassword123",
                PhoneNumber = "555-444-5555",
                Address = "101 Delete Rd.",
                EmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RoleId = MockRoleTestData.Ids.CustomerRoleId // Add the role ID
            };

            // Use EnsureUserWithRoleAndTokenAsync to properly set up the user with role and onboarding
            await EnsureUserWithRoleAndTokenAsync(userToDelete);

            // Verify user exists before deletion
            var userBeforeDelete = await _repository.GetUserByIdAsync(userToDelete.Id);
            Assert.NotNull(userBeforeDelete);

            // Act
            await _repository.DeleteUserAsync(userToDelete);
            await _repository.SaveUserChangesAsync();

            // Attempt to retrieve the deleted user
            var deletedUser = await _repository.GetUserByIdAsync(userToDelete.Id);

            // Assert
            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task GetByEmailAsync_WithValidEmail_ReturnsUser() {
            // Arrange
            var mockUser = MockUserTestData.AdminUser;

            // Ensure user exists using helper
            await EnsureUserWithRoleAndTokenAsync(mockUser);

            // Act
            var user = await _repository.GetUserByEmailAsync(mockUser.Email);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(mockUser.Email, user.Email);
            Assert.Equal(mockUser.FullName, user.FullName);
            Assert.Equal(mockUser.Id, user.Id);
        }

        [Fact]
        public async Task GetByEmailAsync_WithInvalidEmail_ReturnsNull() {
            // Arrange
            string invalidEmail = "nonexistent@example.com";

            // Act
            var user = await _repository.GetUserByEmailAsync(invalidEmail);

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task GetByAccessTokenAsync_ReturnsCorrectUser() {
            // Arrange - Create a user with an auth token
            var user = new User {
                Id = Guid.NewGuid(),
                Email = $"token_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
                FullName = "Token Test User",
                PasswordHash = "hashedpassword123",
                PhoneNumber = "555-666-7777",
                Address = "200 Token Blvd.",
                EmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RoleId = MockRoleTestData.Ids.CustomerRoleId
            };

            // Ensure user exists with a token
            await EnsureUserWithRoleAndTokenAsync(user);

            // Find the token we just created
            var authToken = await _fixture.DbContext.AuthTokens
                .FirstAsync(t => t.UserId == user.Id);

            // Act
            var foundUser = await _repository.GetUserByAccessTokenAsync(authToken.AccessToken);

            // Assert
            Assert.NotNull(foundUser);
            Assert.Equal(user.Id, foundUser.Id);
            Assert.Equal(user.Email, foundUser.Email);
        }
    }
}