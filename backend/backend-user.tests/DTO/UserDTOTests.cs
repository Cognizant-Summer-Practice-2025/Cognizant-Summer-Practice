using Xunit;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using backend_user.DTO.User.Request;
using backend_user.DTO.User.Response;

namespace backend_user.tests.DTO
{
    public class UserDTOTests
    {
        #region RegisterUserRequest Tests

        [Fact]
        public void RegisterUserRequest_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                ProfessionalTitle = "Software Engineer",
                Bio = "A passionate developer",
                Location = "New York",
                ProfileImage = "https://example.com/image.jpg"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void RegisterUserRequest_WithMissingEmail_ShouldFailValidation()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "", // Missing email
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("Email"));
        }

        [Fact]
        public void RegisterUserRequest_WithInvalidEmail_ShouldFailValidation()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "invalid-email", // Invalid email format
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("Email"));
        }

        [Fact]
        public void RegisterUserRequest_WithMissingFirstName_ShouldFailValidation()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                FirstName = "", // Missing first name
                LastName = "Doe"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void RegisterUserRequest_WithTooLongFirstName_ShouldFailValidation()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                FirstName = new string('A', 101), // Too long (over 100 characters)
                LastName = "Doe"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void RegisterUserRequest_WithTooLongProfessionalTitle_ShouldFailValidation()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                ProfessionalTitle = new string('A', 201) // Too long (over 200 characters)
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("ProfessionalTitle"));
        }

        [Fact]
        public void RegisterUserRequest_WithOptionalFields_ShouldPassValidation()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
                // Optional fields are null/empty
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        #endregion

        #region UpdateUserRequest Tests

        [Fact]
        public void UpdateUserRequest_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var request = new UpdateUserRequest
            {
                FirstName = "John",
                LastName = "Doe",
                ProfessionalTitle = "Senior Developer",
                Bio = "Updated bio",
                Location = "San Francisco",
                ProfileImage = "https://example.com/new-image.jpg"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void UpdateUserRequest_WithAllNullFields_ShouldPassValidation()
        {
            // Arrange
            var request = new UpdateUserRequest
            {
                // All fields are null - should be valid for partial updates
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void UpdateUserRequest_WithTooLongFirstName_ShouldFailValidation()
        {
            // Arrange
            var request = new UpdateUserRequest
            {
                FirstName = new string('A', 101) // Too long
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void UpdateUserRequest_WithTooLongLocation_ShouldFailValidation()
        {
            // Arrange
            var request = new UpdateUserRequest
            {
                Location = new string('A', 101) // Too long
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("Location"));
        }

        [Fact]
        public void UpdateUserRequest_WithPartialUpdate_ShouldPassValidation()
        {
            // Arrange
            var request = new UpdateUserRequest
            {
                FirstName = "UpdatedName"
                // Only updating first name, other fields remain null
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        #endregion

        #region Property Tests

        [Fact]
        public void RegisterUserRequest_Properties_ShouldHaveCorrectDefaultValues()
        {
            // Arrange & Act
            var request = new RegisterUserRequest();

            // Assert
            request.Email.Should().Be(string.Empty);
            request.FirstName.Should().Be(string.Empty);
            request.LastName.Should().Be(string.Empty);
            request.ProfessionalTitle.Should().BeNull();
            request.Bio.Should().BeNull();
            request.Location.Should().BeNull();
            request.ProfileImage.Should().BeNull();
        }

        [Fact]
        public void UpdateUserRequest_Properties_ShouldHaveCorrectDefaultValues()
        {
            // Arrange & Act
            var request = new UpdateUserRequest();

            // Assert
            request.FirstName.Should().BeNull();
            request.LastName.Should().BeNull();
            request.ProfessionalTitle.Should().BeNull();
            request.Bio.Should().BeNull();
            request.Location.Should().BeNull();
            request.ProfileImage.Should().BeNull();
        }

        [Fact]
        public void RegisterUserRequest_PropertiesSetAndGet_ShouldWorkCorrectly()
        {
            // Arrange
            var request = new RegisterUserRequest();
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";

            // Act
            request.Email = email;
            request.FirstName = firstName;
            request.LastName = lastName;

            // Assert
            request.Email.Should().Be(email);
            request.FirstName.Should().Be(firstName);
            request.LastName.Should().Be(lastName);
        }

        #endregion

        #region Helper Methods

        private static List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }

        #endregion

        #region Email Validation Edge Cases

        [Theory]
        [InlineData("user@domain.com")]
        [InlineData("user.name@domain.com")]
        [InlineData("user+tag@domain.co.uk")]
        [InlineData("123@domain.com")]
        public void RegisterUserRequest_WithValidEmails_ShouldPassValidation(string email)
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = email,
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("@domain.com")]
        [InlineData("user@")]
        [InlineData("")]
        [InlineData(" ")]
        public void RegisterUserRequest_WithInvalidEmails_ShouldFailValidation(string email)
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = email,
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
        }

        #endregion
    }
}
