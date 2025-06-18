using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Diagnostics.Data;
using Truckero.Infrastructure.Repositories;
using Xunit;

namespace Truckero.Infrastructure.Tests.Integration {
    public class UserRepositoryIntegrationTests : IntegrationTestBase {
        private readonly UserRepository _repository;

        public UserRepositoryIntegrationTests() {
            _repository = new UserRepository(DbContext);
        }

        [Fact]
        public async Task TransactionCommit_SavesChanges() {
            // Arrange - Create a user based on the mock data template
            var templateUser = MockUserTestData.DriverUser;
            var newUser = new User {
                Id = Guid.NewGuid(),
                Email = $"transaction_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com", // Ensure unique email
                FullName = "Transaction Test User",
                PasswordHash = templateUser.PasswordHash,
                PhoneNumber = "555-111-3333",
                Address = "456 Transaction St.",
                EmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            using (var transaction = await DbContext.Database.BeginTransactionAsync()) {
                await _repository.AddUserAsync(newUser);
                await _repository.SaveUserChangesAsync();
                await transaction.CommitAsync();
            }

            // Assert
            var savedUser = await _repository.GetUserByIdAsync(newUser.Id);
            Assert.NotNull(savedUser);
            Assert.Equal(newUser.Email, savedUser.Email);
            Assert.Equal(newUser.FullName, savedUser.FullName);
            Assert.Equal(newUser.PhoneNumber, savedUser.PhoneNumber);
        }

        [Fact]
        public async Task TransactionRollback_DiscardsChanges() {
            // Arrange - Get all users before the test
            var users = await DbContext.Users.ToListAsync();
            var initialCount = users.Count;

            // Create a user based on the mock data template
            var templateUser = MockUserTestData.CustomerUser;
            var newUser = new User {
                Id = Guid.NewGuid(),
                Email = $"rollback_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com", // Ensure unique email
                FullName = "Rollback Test User",
                PasswordHash = templateUser.PasswordHash,
                PhoneNumber = "555-222-4444",
                Address = "789 Rollback Ave.",
                EmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            using (var transaction = await DbContext.Database.BeginTransactionAsync()) {
                await _repository.AddUserAsync(newUser);
                await _repository.SaveUserChangesAsync();
                // Do not commit, let it roll back
            }

            // Assert
            var afterUsers = await DbContext.Users.ToListAsync();
            var afterCount = afterUsers.Count;
            var user = await _repository.GetUserByIdAsync(newUser.Id);

            Assert.Equal(initialCount, afterCount); // Count should remain the same
            Assert.Null(user); // User should not be persisted
        }

        [Fact]
        public async Task ConcurrentOperations_MaintainsDataIntegrity() {
            // Arrange - Add two test users from mock data
            var user1 = MockUserTestData.DriverUser;
            var user2 = MockUserTestData.CustomerUser;

            // Ensure unique emails for the test
            user1.Email = $"driver_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com";
            user2.Email = $"customer_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com";

            // Add both users
            await _repository.AddUserAsync(user1);
            await _repository.AddUserAsync(user2);
            await _repository.SaveUserChangesAsync();

            // Act & Assert - Test concurrent operations
            await Task.WhenAll(
                Task.Run(async () => {
                    using var transaction = await DbContext.Database.BeginTransactionAsync();
                    var userToUpdate = await _repository.GetUserByIdAsync(user1.Id);
                    if (userToUpdate != null) {
                        userToUpdate.FullName = "Updated Driver Name";
                        await _repository.SaveUserChangesAsync();
                        await transaction.CommitAsync();
                    }
                }),
                Task.Run(async () => {
                    using var transaction = await DbContext.Database.BeginTransactionAsync();
                    var userToUpdate = await _repository.GetUserByIdAsync(user2.Id);
                    if (userToUpdate != null) {
                        userToUpdate.FullName = "Updated Customer Name";
                        await _repository.SaveUserChangesAsync();
                        await transaction.CommitAsync();
                    }
                })
            );

            // Verify both updates were applied
            var updatedUser1 = await _repository.GetUserByIdAsync(user1.Id);
            var updatedUser2 = await _repository.GetUserByIdAsync(user2.Id);

            Assert.NotNull(updatedUser1);
            Assert.NotNull(updatedUser2);
            Assert.Equal("Updated Driver Name", updatedUser1.FullName);
            Assert.Equal("Updated Customer Name", updatedUser2.FullName);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsCorrectUser() {
            // Arrange - Add a user with a specific email
            var email = $"unique_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com";
            var user = new User {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = "Email Test User",
                PasswordHash = "hashedpassword123",
                PhoneNumber = "555-333-5555",
                Address = "123 Email St.",
                EmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddUserAsync(user);
            await _repository.SaveUserChangesAsync();

            // Act
            var foundUser = await _repository.GetUserByEmailAsync(email);

            // Assert
            Assert.NotNull(foundUser);
            Assert.Equal(user.Id, foundUser.Id);
            Assert.Equal(email, foundUser.Email);
            Assert.Equal("Email Test User", foundUser.FullName);
        }

        [Fact]
        public async Task DeleteUserAsync_RemovesUser() {
            // Arrange - Add a user to delete
            var userToDelete = new User {
                Id = Guid.NewGuid(),
                Email = $"delete_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
                FullName = "Delete Test User",
                PasswordHash = "hashedpassword123",
                PhoneNumber = "555-444-6666",
                Address = "456 Delete Dr.",
                EmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddUserAsync(userToDelete);
            await _repository.SaveUserChangesAsync();

            // Verify user exists before deletion
            var userBeforeDelete = await _repository.GetUserByIdAsync(userToDelete.Id);
            Assert.NotNull(userBeforeDelete);

            // Act
            await _repository.DeleteUserAsync(userToDelete);
            await _repository.SaveUserChangesAsync();

            // Assert
            var userAfterDelete = await _repository.GetUserByIdAsync(userToDelete.Id);
            Assert.Null(userAfterDelete);
        }
    }
}