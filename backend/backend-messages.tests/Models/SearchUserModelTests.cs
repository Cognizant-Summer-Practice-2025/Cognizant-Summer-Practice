using System;
using BackendMessages.Models;
using FluentAssertions;
using Xunit;

namespace BackendMessages.Tests.Models
{
    public class SearchUserModelTests
    {
        [Fact]
        public void SearchUser_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var searchUser = new SearchUser();

            // Assert
            searchUser.Id.Should().BeEmpty();
            searchUser.Username.Should().BeEmpty();
            searchUser.FirstName.Should().BeNull();
            searchUser.LastName.Should().BeNull();
            searchUser.FullName.Should().BeEmpty();
            searchUser.ProfessionalTitle.Should().BeNull();
            searchUser.AvatarUrl.Should().BeNull();
            searchUser.IsActive.Should().BeFalse();
        }

        [Fact]
        public void SearchUser_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var username = "testuser";
            var firstName = "John";
            var lastName = "Doe";
            var fullName = "John Doe";
            var professionalTitle = "Software Engineer";
            var avatarUrl = "https://example.com/avatar.jpg";
            var isActive = true;

            // Act
            var searchUser = new SearchUser
            {
                Id = id,
                Username = username,
                FirstName = firstName,
                LastName = lastName,
                FullName = fullName,
                ProfessionalTitle = professionalTitle,
                AvatarUrl = avatarUrl,
                IsActive = isActive
            };

            // Assert
            searchUser.Id.Should().Be(id);
            searchUser.Username.Should().Be(username);
            searchUser.FirstName.Should().Be(firstName);
            searchUser.LastName.Should().Be(lastName);
            searchUser.FullName.Should().Be(fullName);
            searchUser.ProfessionalTitle.Should().Be(professionalTitle);
            searchUser.AvatarUrl.Should().Be(avatarUrl);
            searchUser.IsActive.Should().Be(isActive);
        }

        [Fact]
        public void SearchUser_WithNullOptionalProperties_ShouldBeValid()
        {
            // Arrange
            var searchUser = new SearchUser
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                FirstName = null,
                LastName = null,
                FullName = "Test User",
                ProfessionalTitle = null,
                AvatarUrl = null,
                IsActive = true
            };

            // Act & Assert
            searchUser.FirstName.Should().BeNull();
            searchUser.LastName.Should().BeNull();
            searchUser.ProfessionalTitle.Should().BeNull();
            searchUser.AvatarUrl.Should().BeNull();
        }

        [Fact]
        public void SearchUser_WithEmptyStrings_ShouldBeValid()
        {
            // Arrange
            var searchUser = new SearchUser
            {
                Id = Guid.NewGuid(),
                Username = "",
                FirstName = "",
                LastName = "",
                FullName = "",
                ProfessionalTitle = "",
                AvatarUrl = "",
                IsActive = false
            };

            // Act & Assert
            searchUser.Username.Should().BeEmpty();
            searchUser.FirstName.Should().BeEmpty();
            searchUser.LastName.Should().BeEmpty();
            searchUser.FullName.Should().BeEmpty();
            searchUser.ProfessionalTitle.Should().BeEmpty();
            searchUser.AvatarUrl.Should().BeEmpty();
            searchUser.IsActive.Should().BeFalse();
        }

        [Fact]
        public void SearchUser_WithEmptyGuid_ShouldBeValid()
        {
            // Arrange
            var searchUser = new SearchUser
            {
                Id = Guid.Empty,
                Username = "testuser"
            };

            // Act & Assert
            searchUser.Id.Should().Be(Guid.Empty);
        }

        [Theory]
        [InlineData("john.doe")]
        [InlineData("user123")]
        [InlineData("test_user")]
        [InlineData("user-name")]
        [InlineData("user.name")]
        public void SearchUser_WithDifferentUsernames_ShouldSetCorrectly(string username)
        {
            // Arrange
            var searchUser = new SearchUser();

            // Act
            searchUser.Username = username;

            // Assert
            searchUser.Username.Should().Be(username);
        }

        [Theory]
        [InlineData("John")]
        [InlineData("Mary")]
        [InlineData("Jean-Pierre")]
        [InlineData("José")]
        [InlineData("李")]
        public void SearchUser_WithDifferentFirstNames_ShouldSetCorrectly(string firstName)
        {
            // Arrange
            var searchUser = new SearchUser();

            // Act
            searchUser.FirstName = firstName;

            // Assert
            searchUser.FirstName.Should().Be(firstName);
        }

        [Theory]
        [InlineData("Doe")]
        [InlineData("Smith")]
        [InlineData("O'Connor")]
        [InlineData("García")]
        [InlineData("王")]
        public void SearchUser_WithDifferentLastNames_ShouldSetCorrectly(string lastName)
        {
            // Arrange
            var searchUser = new SearchUser();

            // Act
            searchUser.LastName = lastName;

            // Assert
            searchUser.LastName.Should().Be(lastName);
        }

        [Theory]
        [InlineData("John Doe")]
        [InlineData("Mary Jane Smith")]
        [InlineData("Jean-Pierre Dubois")]
        [InlineData("José María García")]
        [InlineData("李小明")]
        public void SearchUser_WithDifferentFullNames_ShouldSetCorrectly(string fullName)
        {
            // Arrange
            var searchUser = new SearchUser();

            // Act
            searchUser.FullName = fullName;

            // Assert
            searchUser.FullName.Should().Be(fullName);
        }

        [Theory]
        [InlineData("Software Engineer")]
        [InlineData("Product Manager")]
        [InlineData("Data Scientist")]
        [InlineData("UX Designer")]
        [InlineData("DevOps Engineer")]
        public void SearchUser_WithDifferentProfessionalTitles_ShouldSetCorrectly(string professionalTitle)
        {
            // Arrange
            var searchUser = new SearchUser();

            // Act
            searchUser.ProfessionalTitle = professionalTitle;

            // Assert
            searchUser.ProfessionalTitle.Should().Be(professionalTitle);
        }

        [Theory]
        [InlineData("https://example.com/avatar.jpg")]
        [InlineData("https://cdn.example.com/users/123/avatar.png")]
        [InlineData("http://localhost:3000/avatars/user.jpg")]
        [InlineData("data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD")]
        public void SearchUser_WithDifferentAvatarUrls_ShouldSetCorrectly(string avatarUrl)
        {
            // Arrange
            var searchUser = new SearchUser();

            // Act
            searchUser.AvatarUrl = avatarUrl;

            // Assert
            searchUser.AvatarUrl.Should().Be(avatarUrl);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SearchUser_WithDifferentIsActiveValues_ShouldSetCorrectly(bool isActive)
        {
            // Arrange
            var searchUser = new SearchUser();

            // Act
            searchUser.IsActive = isActive;

            // Assert
            searchUser.IsActive.Should().Be(isActive);
        }

        [Fact]
        public void SearchUser_WithLongStrings_ShouldBeValid()
        {
            // Arrange
            var longUsername = new string('a', 100);
            var longFirstName = new string('b', 50);
            var longLastName = new string('c', 50);
            var longFullName = new string('d', 200);
            var longProfessionalTitle = new string('e', 100);
            var longAvatarUrl = new string('f', 500);

            var searchUser = new SearchUser
            {
                Id = Guid.NewGuid(),
                Username = longUsername,
                FirstName = longFirstName,
                LastName = longLastName,
                FullName = longFullName,
                ProfessionalTitle = longProfessionalTitle,
                AvatarUrl = longAvatarUrl,
                IsActive = true
            };

            // Act & Assert
            searchUser.Username.Should().Be(longUsername);
            searchUser.FirstName.Should().Be(longFirstName);
            searchUser.LastName.Should().Be(longLastName);
            searchUser.FullName.Should().Be(longFullName);
            searchUser.ProfessionalTitle.Should().Be(longProfessionalTitle);
            searchUser.AvatarUrl.Should().Be(longAvatarUrl);
        }

        [Fact]
        public void SearchUser_WithSpecialCharacters_ShouldBeValid()
        {
            // Arrange
            var searchUser = new SearchUser
            {
                Id = Guid.NewGuid(),
                Username = "user@domain.com",
                FirstName = "Jean-Pierre",
                LastName = "O'Connor",
                FullName = "Jean-Pierre O'Connor",
                ProfessionalTitle = "Software Engineer (Full-Stack)",
                AvatarUrl = "https://example.com/avatar.jpg?size=150&format=png",
                IsActive = true
            };

            // Act & Assert
            searchUser.Username.Should().Be("user@domain.com");
            searchUser.FirstName.Should().Be("Jean-Pierre");
            searchUser.LastName.Should().Be("O'Connor");
            searchUser.FullName.Should().Be("Jean-Pierre O'Connor");
            searchUser.ProfessionalTitle.Should().Be("Software Engineer (Full-Stack)");
            searchUser.AvatarUrl.Should().Be("https://example.com/avatar.jpg?size=150&format=png");
        }

        [Fact]
        public void SearchUser_WithUnicodeCharacters_ShouldBeValid()
        {
            // Arrange
            var searchUser = new SearchUser
            {
                Id = Guid.NewGuid(),
                Username = "user123",
                FirstName = "José",
                LastName = "García",
                FullName = "José María García",
                ProfessionalTitle = "Ingeniero de Software",
                AvatarUrl = "https://example.com/avatar.jpg",
                IsActive = true
            };

            // Act & Assert
            searchUser.FirstName.Should().Be("José");
            searchUser.LastName.Should().Be("García");
            searchUser.FullName.Should().Be("José María García");
            searchUser.ProfessionalTitle.Should().Be("Ingeniero de Software");
        }

        [Fact]
        public void SearchUser_WithWhitespaceOnlyStrings_ShouldBeValid()
        {
            // Arrange
            var searchUser = new SearchUser
            {
                Id = Guid.NewGuid(),
                Username = "   ",
                FirstName = "   ",
                LastName = "   ",
                FullName = "   ",
                ProfessionalTitle = "   ",
                AvatarUrl = "   ",
                IsActive = false
            };

            // Act & Assert
            searchUser.Username.Should().Be("   ");
            searchUser.FirstName.Should().Be("   ");
            searchUser.LastName.Should().Be("   ");
            searchUser.FullName.Should().Be("   ");
            searchUser.ProfessionalTitle.Should().Be("   ");
            searchUser.AvatarUrl.Should().Be("   ");
        }

        [Fact]
        public void SearchUser_WithNewLinesInStrings_ShouldBeValid()
        {
            // Arrange
            var searchUser = new SearchUser
            {
                Id = Guid.NewGuid(),
                Username = "user\n123",
                FirstName = "John\nDoe",
                LastName = "Smith\nJr",
                FullName = "John\nDoe\nSmith",
                ProfessionalTitle = "Software\nEngineer",
                AvatarUrl = "https://example.com\n/avatar.jpg",
                IsActive = true
            };

            // Act & Assert
            searchUser.Username.Should().Be("user\n123");
            searchUser.FirstName.Should().Be("John\nDoe");
            searchUser.LastName.Should().Be("Smith\nJr");
            searchUser.FullName.Should().Be("John\nDoe\nSmith");
            searchUser.ProfessionalTitle.Should().Be("Software\nEngineer");
            searchUser.AvatarUrl.Should().Be("https://example.com\n/avatar.jpg");
        }
    }
}
