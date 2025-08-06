using Xunit;
using FluentAssertions;
using backend_user.Models;
using backend_user.tests.Helpers;

namespace backend_user.tests.Models
{
    public class UserModelTests
    {
        #region Constructor and Default Values Tests

        [Fact]
        public void User_DefaultConstructor_ShouldSetDefaultValues()
        {
            // Act
            var user = new User();

            // Assert
            user.Id.Should().NotBe(Guid.Empty); // Should auto-generate a GUID
            user.Email.Should().Be(string.Empty);
            user.Username.Should().Be(string.Empty);
            user.FirstName.Should().BeNull();
            user.LastName.Should().BeNull();
            user.ProfessionalTitle.Should().BeNull();
            user.Bio.Should().BeNull();
            user.Location.Should().BeNull();
            user.AvatarUrl.Should().BeNull();
            user.IsActive.Should().BeTrue(); // Default should be true
            user.IsAdmin.Should().BeFalse(); // Default should be false
            user.LastLoginAt.Should().BeNull();
            user.OAuthProviders.Should().NotBeNull().And.BeEmpty();
            user.Newsletters.Should().NotBeNull().And.BeEmpty();
            user.UserAnalytics.Should().NotBeNull().And.BeEmpty();
            user.ReportsCreated.Should().NotBeNull().And.BeEmpty();
            user.ReportsResolved.Should().NotBeNull().And.BeEmpty();
        }

        #endregion

        #region Property Tests

        [Fact]
        public void User_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var user = new User();
            var id = Guid.NewGuid();
            var email = "test@example.com";
            var username = "testuser";
            var firstName = "John";
            var lastName = "Doe";
            var title = "Software Engineer";
            var bio = "A passionate developer";
            var location = "New York";
            var avatarUrl = "https://example.com/avatar.jpg";
            var lastLogin = DateTime.UtcNow;

            // Act
            user.Id = id;
            user.Email = email;
            user.Username = username;
            user.FirstName = firstName;
            user.LastName = lastName;
            user.ProfessionalTitle = title;
            user.Bio = bio;
            user.Location = location;
            user.AvatarUrl = avatarUrl;
            user.IsActive = false;
            user.IsAdmin = true;
            user.LastLoginAt = lastLogin;

            // Assert
            user.Id.Should().Be(id);
            user.Email.Should().Be(email);
            user.Username.Should().Be(username);
            user.FirstName.Should().Be(firstName);
            user.LastName.Should().Be(lastName);
            user.ProfessionalTitle.Should().Be(title);
            user.Bio.Should().Be(bio);
            user.Location.Should().Be(location);
            user.AvatarUrl.Should().Be(avatarUrl);
            user.IsActive.Should().BeFalse();
            user.IsAdmin.Should().BeTrue();
            user.LastLoginAt.Should().Be(lastLogin);
        }

        #endregion

        #region Navigation Properties Tests

        [Fact]
        public void User_OAuthProviders_ShouldSupportAddingItems()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider(user.Id);

            // Act
            user.OAuthProviders.Add(oauthProvider);

            // Assert
            user.OAuthProviders.Should().HaveCount(1);
            user.OAuthProviders.Should().Contain(oauthProvider);
        }

        [Fact]
        public void User_Newsletters_ShouldSupportAddingItems()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var newsletter = TestDataFactory.CreateValidNewsletter(user.Id);

            // Act
            user.Newsletters.Add(newsletter);

            // Assert
            user.Newsletters.Should().HaveCount(1);
            user.Newsletters.Should().Contain(newsletter);
        }

        [Fact]
        public void User_UserAnalytics_ShouldSupportAddingItems()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var analytics = TestDataFactory.CreateValidUserAnalytics(user.Id);

            // Act
            user.UserAnalytics.Add(analytics);

            // Assert
            user.UserAnalytics.Should().HaveCount(1);
            user.UserAnalytics.Should().Contain(analytics);
        }

        [Fact]
        public void User_UserReports_ShouldSupportAddingItems()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var report = TestDataFactory.CreateValidUserReport(userId: user.Id);

            // Act
            user.UserReports.Add(report);

            // Assert
            user.UserReports.Should().HaveCount(1);
            user.UserReports.Should().Contain(report);
        }

        [Fact]
        public void User_UserReports_ShouldAllowMultipleReports()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var report1 = TestDataFactory.CreateValidUserReport(userId: user.Id);
            var report2 = TestDataFactory.CreateValidUserReport(userId: user.Id);

            // Act
            user.UserReports.Add(report1);
            user.UserReports.Add(report2);

            // Assert
            user.UserReports.Should().HaveCount(2);
            user.UserReports.Should().Contain(report1);
            user.UserReports.Should().Contain(report2);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void User_WithSameId_ShouldBeEqual()
        {
            // Arrange
            var id = Guid.NewGuid();
            var user1 = TestDataFactory.CreateValidUser();
            var user2 = TestDataFactory.CreateValidUser();
            user1.Id = id;
            user2.Id = id;

            // Act & Assert
            user1.Id.Should().Be(user2.Id);
        }

        [Fact]
        public void User_WithDifferentId_ShouldNotBeEqual()
        {
            // Arrange
            var user1 = TestDataFactory.CreateValidUser();
            var user2 = TestDataFactory.CreateValidUser();

            // Act & Assert
            user1.Id.Should().NotBe(user2.Id);
        }

        #endregion

        #region State Tests

        [Fact]
        public void User_ActiveUser_ShouldHaveCorrectState()
        {
            // Arrange & Act
            var user = TestDataFactory.CreateValidUser(isActive: true);

            // Assert
            user.IsActive.Should().BeTrue();
        }

        [Fact]
        public void User_InactiveUser_ShouldHaveCorrectState()
        {
            // Arrange & Act
            var user = TestDataFactory.CreateValidUser(isActive: false);

            // Assert
            user.IsActive.Should().BeFalse();
        }

        [Fact]
        public void User_AdminUser_ShouldHaveCorrectPermissions()
        {
            // Arrange & Act
            var user = TestDataFactory.CreateValidUser(isAdmin: true);

            // Assert
            user.IsAdmin.Should().BeTrue();
        }

        [Fact]
        public void User_RegularUser_ShouldHaveCorrectPermissions()
        {
            // Arrange & Act
            var user = TestDataFactory.CreateValidUser(isAdmin: false);

            // Assert
            user.IsAdmin.Should().BeFalse();
        }

        #endregion

        #region Complex Scenario Tests

        [Fact]
        public void User_WithMultipleOAuthProviders_ShouldMaintainCorrectRelationships()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var googleProvider = TestDataFactory.CreateValidOAuthProvider(user.Id, OAuthProviderType.Google);
            var githubProvider = TestDataFactory.CreateValidOAuthProvider(user.Id, OAuthProviderType.GitHub);

            // Act
            user.OAuthProviders.Add(googleProvider);
            user.OAuthProviders.Add(githubProvider);

            // Assert
            user.OAuthProviders.Should().HaveCount(2);
            user.OAuthProviders.Should().Contain(p => p.Provider == OAuthProviderType.Google);
            user.OAuthProviders.Should().Contain(p => p.Provider == OAuthProviderType.GitHub);
            user.OAuthProviders.All(p => p.UserId == user.Id).Should().BeTrue();
        }

        [Fact]
        public void User_WithCompleteProfile_ShouldHaveAllFieldsPopulated()
        {
            // Arrange & Act
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "complete@example.com",
                Username = "completeuser",
                FirstName = "Complete",
                LastName = "User",
                ProfessionalTitle = "Senior Software Engineer",
                Bio = "A comprehensive user profile with all fields populated",
                Location = "San Francisco, CA",
                AvatarUrl = "https://example.com/complete-avatar.jpg",
                IsActive = true,
                IsAdmin = false,
                LastLoginAt = DateTime.UtcNow.AddMinutes(-5)
            };

            // Assert
            user.Id.Should().NotBe(Guid.Empty);
            user.Email.Should().NotBeNullOrEmpty();
            user.Username.Should().NotBeNullOrEmpty();
            user.FirstName.Should().NotBeNullOrEmpty();
            user.LastName.Should().NotBeNullOrEmpty();
            user.ProfessionalTitle.Should().NotBeNullOrEmpty();
            user.Bio.Should().NotBeNullOrEmpty();
            user.Location.Should().NotBeNullOrEmpty();
            user.AvatarUrl.Should().NotBeNullOrEmpty();
            user.IsActive.Should().BeTrue();
            user.LastLoginAt.Should().NotBeNull();
        }

        #endregion

        #region Collection Navigation Properties Tests

        [Fact]
        public void User_OAuthProviders_ShouldAllowAddAndRemove()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider(user.Id);

            // Act
            user.OAuthProviders.Add(oauthProvider);

            // Assert
            user.OAuthProviders.Should().HaveCount(1);
            user.OAuthProviders.First().Should().Be(oauthProvider);
            
            // Act - Remove
            user.OAuthProviders.Remove(oauthProvider);
            
            // Assert
            user.OAuthProviders.Should().BeEmpty();
        }

        [Fact]
        public void User_Newsletters_ShouldAllowAddAndRemove()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var newsletter = TestDataFactory.CreateValidNewsletter(user.Id);

            // Act
            user.Newsletters.Add(newsletter);

            // Assert
            user.Newsletters.Should().HaveCount(1);
            user.Newsletters.First().Should().Be(newsletter);
            
            // Act - Remove
            user.Newsletters.Remove(newsletter);
            
            // Assert
            user.Newsletters.Should().BeEmpty();
        }

        [Fact]
        public void User_UserAnalytics_ShouldAllowAddAndRemove()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var analytics = TestDataFactory.CreateValidUserAnalytics(user.Id);

            // Act
            user.UserAnalytics.Add(analytics);

            // Assert
            user.UserAnalytics.Should().HaveCount(1);
            user.UserAnalytics.First().Should().Be(analytics);
            
            // Act - Remove
            user.UserAnalytics.Remove(analytics);
            
            // Assert
            user.UserAnalytics.Should().BeEmpty();
        }

        [Fact]
        public void User_UserReports_ShouldAllowAddAndRemove()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var report = TestDataFactory.CreateValidUserReport(userId: user.Id);

            // Act
            user.UserReports.Add(report);

            // Assert
            user.UserReports.Should().HaveCount(1);
            user.UserReports.First().Should().Be(report);
            
            // Act - Remove
            user.UserReports.Remove(report);
            
            // Assert
            user.UserReports.Should().BeEmpty();
        }

        [Fact]
        public void User_UserReports_ShouldMaintainReferentialIntegrity()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var reporter = TestDataFactory.CreateValidUser();
            var report = TestDataFactory.CreateValidUserReport(userId: user.Id, reportedByUserId: reporter.Id);

            // Act
            user.UserReports.Add(report);

            // Assert
            user.UserReports.Should().HaveCount(1);
            user.UserReports.First().Should().Be(report);
            user.UserReports.First().UserId.Should().Be(user.Id);
            user.UserReports.First().ReportedByUserId.Should().Be(reporter.Id);
            
            // Act - Remove
            user.UserReports.Remove(report);
            
            // Assert
            user.UserReports.Should().BeEmpty();
        }

        [Fact]
        public void User_MultipleOAuthProviders_ShouldHandleCorrectly()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var googleProvider = TestDataFactory.CreateValidOAuthProvider(user.Id, OAuthProviderType.Google);
            var facebookProvider = TestDataFactory.CreateValidOAuthProvider(user.Id, OAuthProviderType.Facebook);
            var githubProvider = TestDataFactory.CreateValidOAuthProvider(user.Id, OAuthProviderType.GitHub);

            // Act
            user.OAuthProviders.Add(googleProvider);
            user.OAuthProviders.Add(facebookProvider);
            user.OAuthProviders.Add(githubProvider);

            // Assert
            user.OAuthProviders.Should().HaveCount(3);
            user.OAuthProviders.Should().Contain(googleProvider);
            user.OAuthProviders.Should().Contain(facebookProvider);
            user.OAuthProviders.Should().Contain(githubProvider);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void User_IsActive_ShouldSetCorrectly(bool isActive)
        {
            // Arrange & Act
            var user = new User { IsActive = isActive };

            // Assert
            user.IsActive.Should().Be(isActive);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void User_IsAdmin_ShouldSetCorrectly(bool isAdmin)
        {
            // Arrange & Act
            var user = new User { IsAdmin = isAdmin };

            // Assert
            user.IsAdmin.Should().Be(isAdmin);
        }

        [Fact]
        public void User_TestDataFactory_WithCustomParameters_ShouldUseProvidedValues()
        {
            // Arrange
            var customEmail = "custom@test.com";
            var customUsername = "customuser";

            // Act
            var user = TestDataFactory.CreateValidUser(customEmail, customUsername, false, true);

            // Assert
            user.Email.Should().Be(customEmail);
            user.Username.Should().Be(customUsername);
            user.IsActive.Should().BeFalse();
            user.IsAdmin.Should().BeTrue();
        }

        #endregion
    }
}
