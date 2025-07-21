using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using backend_user.Repositories;
using backend_user.Models;
using backend_user.Data;
using backend_user.DTO.User.Request;
using backend_user.tests.Helpers;

namespace backend_user.tests.Repositories
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly UserDbContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            _context = TestDbContextHelper.CreateInMemoryContext();
            _repository = new UserRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region GetUserById Tests

        [Fact]
        public async Task GetUserById_WithExistingUser_ShouldReturnUser()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserById(user.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(user.Id);
            result.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetUserById_WithNonExistentUser_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.GetUserById(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetUserByEmail Tests

        [Fact]
        public async Task GetUserByEmail_WithExistingUser_ShouldReturnUser()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser(email: "test@example.com");
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserByEmail(user.Email);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(user.Email);
            result.Id.Should().Be(user.Id);
        }

        [Fact]
        public async Task GetUserByEmail_WithNonExistentEmail_ShouldReturnNull()
        {
            // Arrange
            var nonExistentEmail = "nonexistent@example.com";

            // Act
            var result = await _repository.GetUserByEmail(nonExistentEmail);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByEmail_WithCaseDifferentEmail_ShouldReturnNull()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser(email: "test@example.com");
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserByEmail("TEST@EXAMPLE.COM");

            // Assert
            // InMemory database is case-sensitive, unlike real SQL Server which can be case-insensitive
            result.Should().BeNull();
        }

        #endregion

        #region GetAllUsers Tests

        [Fact]
        public async Task GetAllUsers_WithMultipleUsers_ShouldReturnAllUsers()
        {
            // Arrange
            var users = TestDataFactory.CreateUserList(3);
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllUsers();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(users, options => options.Excluding(u => u.OAuthProviders)
                .Excluding(u => u.Newsletters)
                .Excluding(u => u.UserAnalytics)
                .Excluding(u => u.ReportsCreated)
                .Excluding(u => u.ReportsResolved));
        }

        [Fact]
        public async Task GetAllUsers_WithNoUsers_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetAllUsers();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllUsers_WithInactiveUsers_ShouldReturnAllUsers()
        {
            // Arrange
            var activeUser = TestDataFactory.CreateValidUser(isActive: true);
            var inactiveUser = TestDataFactory.CreateValidUser(isActive: false);
            
            await _context.Users.AddRangeAsync(activeUser, inactiveUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllUsers();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(u => u.IsActive);
            result.Should().Contain(u => !u.IsActive);
        }

        #endregion

        #region CreateUser Tests

        [Fact]
        public async Task CreateUser_WithValidUser_ShouldCreateAndReturnUser()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();

            // Act
            var result = await _repository.CreateUser(user);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            
            // Verify user was actually saved to database
            var savedUser = await _context.Users.FindAsync(user.Id);
            savedUser.Should().NotBeNull();
            savedUser!.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task CreateUser_WithDuplicateEmail_ShouldAllowBothUsers()
        {
            // Arrange
            var user1 = TestDataFactory.CreateValidUser(email: "duplicate@example.com");
            var user2 = TestDataFactory.CreateValidUser(email: "duplicate@example.com");

            // Act
            var result1 = await _repository.CreateUser(user1);
            var result2 = await _repository.CreateUser(user2);

            // Assert - InMemory DB doesn't enforce unique constraints like real DB
            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
            result1.Id.Should().NotBe(result2.Id); // Different IDs
            result1.Email.Should().Be(result2.Email); // Same email allowed
        }

        #endregion

        #region UpdateUser Tests

        [Fact]
        public async Task UpdateUser_WithExistingUser_ShouldUpdateAndReturnUser()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateUserRequest
            {
                FirstName = "Updated",
                LastName = "Name",
                ProfessionalTitle = "Updated Title"
            };

            // Act
            var result = await _repository.UpdateUser(user.Id, updateRequest);

            // Assert
            result.Should().NotBeNull();
            result!.FirstName.Should().Be("Updated");
            result.LastName.Should().Be("Name");
            result.ProfessionalTitle.Should().Be("Updated Title");

            // Verify changes were saved
            var updatedUser = await _context.Users.FindAsync(user.Id);
            updatedUser!.FirstName.Should().Be("Updated");
        }

        [Fact]
        public async Task UpdateUser_WithNonExistentUser_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateRequest = new UpdateUserRequest
            {
                FirstName = "Updated"
            };

            // Act
            var result = await _repository.UpdateUser(nonExistentId, updateRequest);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUser_WithPartialUpdate_ShouldUpdateOnlySpecifiedFields()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            user.FirstName = "Original";
            user.LastName = "User";
            
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateUserRequest
            {
                FirstName = "Updated"
                // LastName is not provided, should remain unchanged
            };

            // Act
            var result = await _repository.UpdateUser(user.Id, updateRequest);

            // Assert
            result.Should().NotBeNull();
            result!.FirstName.Should().Be("Updated");
            result.LastName.Should().Be("User"); // Should remain unchanged
        }

        #endregion

        #region UpdateLastLoginAsync Tests

        [Fact]
        public async Task UpdateLastLoginAsync_WithExistingUser_ShouldUpdateLastLoginAndReturnTrue()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var newLastLogin = DateTime.UtcNow;

            // Act
            var result = await _repository.UpdateLastLoginAsync(user.Id, newLastLogin);

            // Assert
            result.Should().BeTrue();

            // Verify the update was saved
            var updatedUser = await _context.Users.FindAsync(user.Id);
            updatedUser!.LastLoginAt.Should().BeCloseTo(newLastLogin, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task UpdateLastLoginAsync_WithNonExistentUser_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var newLastLogin = DateTime.UtcNow;

            // Act
            var result = await _repository.UpdateLastLoginAsync(nonExistentId, newLastLogin);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Database Transaction Tests

        [Fact]
        public async Task CreateUser_WhenDatabaseFails_ShouldThrowException()
        {
            // Arrange
            await _context.DisposeAsync(); // Dispose the context to simulate database failure
            var user = TestDataFactory.CreateValidUser();

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
                await _repository.CreateUser(user));
        }

        #endregion

        #region Edge Cases and Error Handling Tests

        [Fact]
        public async Task GetUserById_WithEmptyGuid_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetUserById(Guid.Empty);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByEmail_WithNullEmail_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetUserByEmail(null!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByEmail_WithEmptyEmail_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetUserByEmail(string.Empty);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByEmail_WithWhitespaceEmail_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetUserByEmail("   ");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateUser_WithNullUser_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _repository.CreateUser(null!));
        }

        [Fact]
        public async Task UpdateUser_WithNullRequest_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.UpdateUser(userId, null!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateLastLoginAsync_WithEmptyGuid_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.UpdateLastLoginAsync(Guid.Empty, DateTime.UtcNow);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateLastLoginAsync_WithFutureDate_ShouldUpdateCorrectly()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            
            var futureDate = DateTime.UtcNow.AddHours(1);

            // Act
            var result = await _repository.UpdateLastLoginAsync(user.Id, futureDate);

            // Assert
            result.Should().BeTrue();
            
            var updatedUser = await _repository.GetUserById(user.Id);
            updatedUser!.LastLoginAt.Should().BeCloseTo(futureDate, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Repository_ConcurrentAccess_ShouldHandleCorrectly()
        {
            // Arrange
            var users = TestDataFactory.CreateUserList(5);
            var tasks = new List<Task>();

            // Act - Add users concurrently
            foreach (var user in users)
            {
                tasks.Add(_repository.CreateUser(user));
            }

            await Task.WhenAll(tasks);

            // Assert
            var allUsers = await _repository.GetAllUsers();
            allUsers.Should().HaveCount(5);
        }

        [Fact]
        public async Task Repository_LargeDataSet_ShouldPerformCorrectly()
        {
            // Arrange
            var users = TestDataFactory.CreateUserList(100);

            // Act
            foreach (var user in users)
            {
                await _repository.CreateUser(user);
            }

            // Assert
            var allUsers = await _repository.GetAllUsers();
            allUsers.Should().HaveCount(100);

            // Verify we can find specific users
            var firstUser = users.First();
            var foundUser = await _repository.GetUserById(firstUser.Id);
            foundUser.Should().NotBeNull();
            foundUser!.Email.Should().Be(firstUser.Email);
        }

        [Fact]
        public async Task UpdateUser_WithCompleteProfile_ShouldUpdateAllFields()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateUserRequest
            {
                FirstName = "UpdatedFirst",
                LastName = "UpdatedLast",
                ProfessionalTitle = "Updated Title",
                Bio = "Updated bio description",
                Location = "Updated Location",
                ProfileImage = "https://updated.com/avatar.jpg"
            };

            // Act
            var result = await _repository.UpdateUser(user.Id, updateRequest);

            // Assert
            result.Should().NotBeNull();
            result!.FirstName.Should().Be("UpdatedFirst");
            result.LastName.Should().Be("UpdatedLast");
            result.ProfessionalTitle.Should().Be("Updated Title");
            result.Bio.Should().Be("Updated bio description");
            result.Location.Should().Be("Updated Location");
            result.AvatarUrl.Should().Be("https://updated.com/avatar.jpg");
        }

        [Fact]
        public async Task GetAllUsers_Performance_ShouldCompleteQuickly()
        {
            // Arrange
            var users = TestDataFactory.CreateUserList(50);
            foreach (var user in users)
            {
                await _repository.CreateUser(user);
            }

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await _repository.GetAllUsers();
            stopwatch.Stop();

            // Assert
            result.Should().HaveCount(50);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete in less than 1 second
        }

        #endregion
    }
}
