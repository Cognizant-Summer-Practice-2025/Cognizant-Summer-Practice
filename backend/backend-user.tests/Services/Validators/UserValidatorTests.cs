using Xunit;
using FluentAssertions;
using backend_user.Services.Validators;
using backend_user.DTO.User.Request;

namespace backend_user.tests.Services.Validators
{
    public class UserValidatorTests
    {
        #region ValidateRegisterRequest Tests

        [Fact]
        public void ValidateRegisterRequest_WithNullRequest_ShouldReturnFailure()
        {
            // Act
            var result = UserValidator.ValidateRegisterRequest(null!);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Request cannot be null");
        }

        [Fact]
        public void ValidateRegisterRequest_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = UserValidator.ValidateRegisterRequest(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void ValidateRegisterRequest_WithInvalidEmail_ShouldReturnFailure(string email)
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = email!,
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = UserValidator.ValidateRegisterRequest(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Email is required");
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("@domain.com")]
        [InlineData("user@")]
        public void ValidateRegisterRequest_WithInvalidEmailFormat_ShouldReturnFailure(string email)
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = email,
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = UserValidator.ValidateRegisterRequest(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Email format is invalid");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void ValidateRegisterRequest_WithInvalidFirstName_ShouldReturnFailure(string firstName)
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                FirstName = firstName!,
                LastName = "Doe"
            };

            // Act
            var result = UserValidator.ValidateRegisterRequest(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("First name is required");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void ValidateRegisterRequest_WithInvalidLastName_ShouldReturnFailure(string lastName)
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = lastName!
            };

            // Act
            var result = UserValidator.ValidateRegisterRequest(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Last name is required");
        }

        [Fact]
        public void ValidateRegisterRequest_WithTooLongProfessionalTitle_ShouldReturnFailure()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                ProfessionalTitle = new string('A', 201) // Exceeds 200 characters
            };

            // Act
            var result = UserValidator.ValidateRegisterRequest(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Professional title cannot exceed 200 characters");
        }

        [Fact]
        public void ValidateRegisterRequest_WithTooLongLocation_ShouldReturnFailure()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Location = new string('A', 101) // Exceeds 100 characters
            };

            // Act
            var result = UserValidator.ValidateRegisterRequest(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Location cannot exceed 100 characters");
        }

        [Fact]
        public void ValidateRegisterRequest_WithMultipleErrors_ShouldReturnAllErrors()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "invalid-email",
                FirstName = "",
                LastName = "",
                ProfessionalTitle = new string('A', 201),
                Location = new string('B', 101)
            };

            // Act
            var result = UserValidator.ValidateRegisterRequest(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(5);
            result.Errors.Should().Contain("Email format is invalid");
            result.Errors.Should().Contain("First name is required");
            result.Errors.Should().Contain("Last name is required");
            result.Errors.Should().Contain("Professional title cannot exceed 200 characters");
            result.Errors.Should().Contain("Location cannot exceed 100 characters");
        }

        #endregion

        #region ValidateUpdateRequest Tests

        [Fact]
        public void ValidateUpdateRequest_WithNullRequest_ShouldReturnFailure()
        {
            // Act
            var result = UserValidator.ValidateUpdateRequest(null!);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Request cannot be null");
        }

        [Fact]
        public void ValidateUpdateRequest_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var request = new UpdateUserRequest
            {
                FirstName = "John",
                LastName = "Doe",
                ProfessionalTitle = "Software Engineer",
                Location = "New York"
            };

            // Act
            var result = UserValidator.ValidateUpdateRequest(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateUpdateRequest_WithAllNullFields_ShouldReturnSuccess()
        {
            // Arrange
            var request = new UpdateUserRequest();

            // Act
            var result = UserValidator.ValidateUpdateRequest(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateUpdateRequest_WithTooLongProfessionalTitle_ShouldReturnFailure()
        {
            // Arrange
            var request = new UpdateUserRequest
            {
                ProfessionalTitle = new string('A', 201)
            };

            // Act
            var result = UserValidator.ValidateUpdateRequest(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Professional title cannot exceed 200 characters");
        }

        [Fact]
        public void ValidateUpdateRequest_WithTooLongLocation_ShouldReturnFailure()
        {
            // Arrange
            var request = new UpdateUserRequest
            {
                Location = new string('A', 101)
            };

            // Act
            var result = UserValidator.ValidateUpdateRequest(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Location cannot exceed 100 characters");
        }

        [Fact]
        public void ValidateUpdateRequest_WithEmptyStrings_ShouldReturnSuccess()
        {
            // Arrange
            var request = new UpdateUserRequest
            {
                FirstName = "",
                LastName = "",
                ProfessionalTitle = "",
                Location = ""
            };

            // Act
            var result = UserValidator.ValidateUpdateRequest(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #endregion

        #region ValidateUserId Tests

        [Fact]
        public void ValidateUserId_WithEmptyGuid_ShouldReturnFailure()
        {
            // Act
            var result = UserValidator.ValidateUserId(Guid.Empty);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("User ID cannot be empty");
        }

        [Fact]
        public void ValidateUserId_WithValidGuid_ShouldReturnSuccess()
        {
            // Arrange
            var validGuid = Guid.NewGuid();

            // Act
            var result = UserValidator.ValidateUserId(validGuid);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #endregion

        #region Email Validation Edge Cases

        [Theory]
        [InlineData("user@domain.com")]
        [InlineData("user.name@domain.com")]
        [InlineData("user+tag@domain.co.uk")]
        [InlineData("123@domain.com")]
        [InlineData("user_name@domain-name.com")]
        public void ValidateRegisterRequest_WithValidEmailFormats_ShouldReturnSuccess(string email)
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = email,
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = UserValidator.ValidateRegisterRequest(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #endregion

        #region Boundary Tests

        [Fact]
        public void ValidateRegisterRequest_WithExactly200CharacterProfessionalTitle_ShouldReturnSuccess()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                ProfessionalTitle = new string('A', 200) // Exactly 200 characters
            };

            // Act
            var result = UserValidator.ValidateRegisterRequest(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateRegisterRequest_WithExactly100CharacterLocation_ShouldReturnSuccess()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Location = new string('A', 100) // Exactly 100 characters
            };

            // Act
            var result = UserValidator.ValidateRegisterRequest(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        #endregion
    }
}
