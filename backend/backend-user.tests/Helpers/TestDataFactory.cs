using AutoFixture;
using backend_user.Models;
using backend_user.DTO.User.Request;
using backend_user.DTO.User.Response;
using backend_user.DTO.OAuthProvider.Request;
using backend_user.DTO.Authentication.Request;
using Microsoft.EntityFrameworkCore;
using backend_user.Data;

namespace backend_user.tests.Helpers
{
    /// <summary>
    /// Test data factory for creating test objects with consistent data.
    /// </summary>
    public static class TestDataFactory
    {
        private static readonly Fixture _fixture = new();

        static TestDataFactory()
        {
            // Configure AutoFixture to create valid test data
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public static User CreateValidUser(
            string? email = null,
            string? username = null,
            bool isActive = true,
            bool isAdmin = false)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Email = email ?? _fixture.Create<string>() + "@test.com",
                Username = username ?? _fixture.Create<string>().Substring(0, 10),
                FirstName = _fixture.Create<string>().Substring(0, 15),
                LastName = _fixture.Create<string>().Substring(0, 15),
                ProfessionalTitle = _fixture.Create<string>().Substring(0, 20),
                Bio = _fixture.Create<string>(),
                Location = _fixture.Create<string>().Substring(0, 15),
                AvatarUrl = "https://example.com/avatar.jpg",
                IsActive = isActive,
                IsAdmin = isAdmin,
                LastLoginAt = DateTime.UtcNow.AddDays(-1)
            };
        }

        public static OAuthProvider CreateValidOAuthProvider(
            Guid? userId = null,
            OAuthProviderType? providerType = null,
            string? providerId = null)
        {
            return new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                Provider = providerType ?? OAuthProviderType.Google,
                ProviderId = providerId ?? _fixture.Create<string>(),
                ProviderEmail = _fixture.Create<string>() + "@test.com",
                AccessToken = _fixture.Create<string>(),
                CreatedAt = DateTime.UtcNow
            };
        }

        public static Newsletter CreateValidNewsletter(Guid? userId = null)
        {
            return new Newsletter
            {
                Id = Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                Type = _fixture.Create<string>().Substring(0, 10),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static UserAnalytics CreateValidUserAnalytics(Guid? userId = null)
        {
            return new UserAnalytics
            {
                Id = Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                SessionId = _fixture.Create<string>(),
                EventType = _fixture.Create<string>().Substring(0, 10),
                EventData = "{}",
                CreatedAt = DateTime.UtcNow
            };
        }

        public static UserReport CreateValidUserReport(
            Guid? reporterId = null,
            Guid? resolvedById = null)
        {
            return new UserReport
            {
                Id = Guid.NewGuid(),
                ReporterId = reporterId ?? Guid.NewGuid(),
                ResolvedBy = resolvedById,
                ReportedService = "user-service",
                ReportedType = ReportedType.User,
                ReportedId = Guid.NewGuid(),
                ReportType = ReportType.Spam,
                Description = _fixture.Create<string>(),
                Status = ReportStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ResolvedAt = null
            };
        }

        public static List<User> CreateUserList(int count = 3)
        {
            return Enumerable.Range(0, count)
                .Select(_ => CreateValidUser())
                .ToList();
        }

        // DTO Factory Methods
        public static RegisterUserRequest CreateValidRegisterUserRequest()
        {
            var firstName = _fixture.Create<string>();
            var lastName = _fixture.Create<string>();
            var title = _fixture.Create<string>();
            var bio = _fixture.Create<string>();
            var location = _fixture.Create<string>();
            
            return new RegisterUserRequest
            {
                Email = _fixture.Create<string>() + "@test.com",
                FirstName = firstName.Length > 10 ? firstName.Substring(0, 10) : firstName,
                LastName = lastName.Length > 10 ? lastName.Substring(0, 10) : lastName,
                ProfessionalTitle = title.Length > 20 ? title.Substring(0, 20) : title,
                Bio = bio.Length > 100 ? bio.Substring(0, 100) : bio,
                Location = location.Length > 30 ? location.Substring(0, 30) : location,
                ProfileImage = _fixture.Create<string>()
            };
        }

        public static UpdateUserRequest CreateValidUpdateUserRequest()
        {
            var firstName = _fixture.Create<string>();
            var lastName = _fixture.Create<string>();
            var title = _fixture.Create<string>();
            var bio = _fixture.Create<string>();
            var location = _fixture.Create<string>();
            
            return new UpdateUserRequest
            {
                FirstName = firstName.Length > 10 ? firstName.Substring(0, 10) : firstName,
                LastName = lastName.Length > 10 ? lastName.Substring(0, 10) : lastName,
                ProfessionalTitle = title.Length > 20 ? title.Substring(0, 20) : title,
                Bio = bio.Length > 100 ? bio.Substring(0, 100) : bio,
                Location = location.Length > 30 ? location.Substring(0, 30) : location,
                ProfileImage = _fixture.Create<string>()
            };
        }

        public static UserProfileDto CreateValidUserProfile()
        {
            var username = _fixture.Create<string>();
            var firstName = _fixture.Create<string>();
            var lastName = _fixture.Create<string>();
            var title = _fixture.Create<string>();
            var bio = _fixture.Create<string>();
            var location = _fixture.Create<string>();
            
            return new UserProfileDto
            {
                Id = Guid.NewGuid(),
                Username = username.Length > 15 ? username.Substring(0, 15) : username,
                FirstName = firstName.Length > 10 ? firstName.Substring(0, 10) : firstName,
                LastName = lastName.Length > 10 ? lastName.Substring(0, 10) : lastName,
                ProfessionalTitle = title.Length > 20 ? title.Substring(0, 20) : title,
                Bio = bio.Length > 100 ? bio.Substring(0, 100) : bio,
                Location = location.Length > 30 ? location.Substring(0, 30) : location,
                AvatarUrl = _fixture.Create<string>()
            };
        }

        public static OAuthProviderCreateRequestDto CreateValidOAuthProviderCreateRequest()
        {
            return new OAuthProviderCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                ProviderId = _fixture.Create<string>(),
                ProviderEmail = _fixture.Create<string>() + "@test.com",
                AccessToken = _fixture.Create<string>(),
                RefreshToken = _fixture.Create<string>(),
                TokenExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        public static UserResponseDto CreateValidUserResponseDto()
        {
            var username = _fixture.Create<string>();
            var firstName = _fixture.Create<string>();
            var lastName = _fixture.Create<string>();
            var title = _fixture.Create<string>();
            var bio = _fixture.Create<string>();
            var location = _fixture.Create<string>();
            
            return new UserResponseDto
            {
                Id = Guid.NewGuid(),
                Email = _fixture.Create<string>() + "@test.com",
                Username = username.Length > 15 ? username.Substring(0, 15) : username,
                FirstName = firstName.Length > 10 ? firstName.Substring(0, 10) : firstName,
                LastName = lastName.Length > 10 ? lastName.Substring(0, 10) : lastName,
                ProfessionalTitle = title.Length > 20 ? title.Substring(0, 20) : title,
                Bio = bio.Length > 100 ? bio.Substring(0, 100) : bio,
                Location = location.Length > 30 ? location.Substring(0, 30) : location,
                AvatarUrl = _fixture.Create<string>(),
                IsActive = true,
                IsAdmin = false,
                LastLoginAt = DateTime.UtcNow
            };
        }

        public static OAuthLoginRequestDto CreateValidOAuthLoginRequest(
            OAuthProviderType? provider = null,
            string? providerId = null,
            string? providerEmail = null,
            string? accessToken = null)
        {
            return new OAuthLoginRequestDto
            {
                Provider = provider ?? OAuthProviderType.Google,
                ProviderId = providerId ?? "google_12345",
                ProviderEmail = providerEmail ?? "test@example.com",
                AccessToken = accessToken ?? "access_token_12345"
            };
        }

        public static RegisterOAuthUserRequest CreateValidRegisterOAuthUserRequest(
            OAuthProviderType? provider = null,
            string? providerId = null,
            string? providerEmail = null)
        {
            return new RegisterOAuthUserRequest
            {
                FirstName = "John",
                LastName = "Doe", 
                Email = "john.doe@example.com",
                Provider = provider ?? OAuthProviderType.Google,
                ProviderId = providerId ?? "google_12345",
                ProviderEmail = providerEmail ?? "john.doe@example.com",
                AccessToken = "access_token_12345",
                RefreshToken = "refresh_token_12345"
            };
        }
    }

    /// <summary>
    /// Helper for creating in-memory database contexts for testing.
    /// </summary>
    public static class TestDbContextHelper
    {
        public static UserDbContext CreateInMemoryContext(string? databaseName = null)
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
                .Options;

            return new UserDbContext(options);
        }

        public static async Task<UserDbContext> CreateContextWithDataAsync(params User[] users)
        {
            var context = CreateInMemoryContext();
            
            if (users.Any())
            {
                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }

            return context;
        }
    }
}
