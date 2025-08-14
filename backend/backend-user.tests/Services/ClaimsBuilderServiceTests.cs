using Xunit;
using FluentAssertions;
using System.Security.Claims;
using backend_user.Services;
using backend_user.Models;

namespace backend_user.tests.Services
{
    public class ClaimsBuilderServiceTests
    {
        private readonly ClaimsBuilderService _service;

        public ClaimsBuilderServiceTests()
        {
            _service = new ClaimsBuilderService();
        }

        [Fact]
        public void BuildClaims_ShouldThrowArgumentNullException_WhenUserIsNull()
        {
            // Act & Assert
            var act = () => _service.BuildClaims(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("user");
        }

        [Fact]
        public void BuildClaims_ShouldIncludeRequiredClaims_ForBasicUser()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                IsAdmin = false,
                IsActive = true
            };

            // Act
            var claims = _service.BuildClaims(user).ToList();

            // Assert
            claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
            claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
            claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == user.Username);
            claims.Should().Contain(c => c.Type == "IsAdmin" && c.Value == user.IsAdmin.ToString());
            claims.Should().Contain(c => c.Type == "IsActive" && c.Value == user.IsActive.ToString());
        }

        [Fact]
        public void BuildClaims_ShouldIncludeOptionalClaims_WhenUserHasAllProperties()
        {
            // Arrange
            var lastLogin = DateTime.UtcNow.AddHours(-2);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "John",
                LastName = "Doe",
                ProfessionalTitle = "Software Engineer",
                Location = "New York, NY",
                IsAdmin = true,
                IsActive = true,
                LastLoginAt = lastLogin
            };

            // Act
            var claims = _service.BuildClaims(user).ToList();

            // Assert
            claims.Should().Contain(c => c.Type == ClaimTypes.GivenName && c.Value == user.FirstName);
            claims.Should().Contain(c => c.Type == ClaimTypes.Surname && c.Value == user.LastName);
            claims.Should().Contain(c => c.Type == "ProfessionalTitle" && c.Value == user.ProfessionalTitle);
            claims.Should().Contain(c => c.Type == "Location" && c.Value == user.Location);
            claims.Should().Contain(c => c.Type == "LastLogin" && c.Value == lastLogin.ToString("O"));
        }

        [Fact]
        public void BuildClaims_ShouldNotIncludeOptionalClaims_WhenUserPropertiesAreNull()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                FirstName = null,
                LastName = null,
                ProfessionalTitle = null,
                Location = null,
                IsAdmin = false,
                IsActive = true,
                LastLoginAt = null
            };

            // Act
            var claims = _service.BuildClaims(user).ToList();

            // Assert
            claims.Should().NotContain(c => c.Type == ClaimTypes.GivenName);
            claims.Should().NotContain(c => c.Type == ClaimTypes.Surname);
            claims.Should().NotContain(c => c.Type == "ProfessionalTitle");
            claims.Should().NotContain(c => c.Type == "Location");
            claims.Should().NotContain(c => c.Type == "LastLogin");
        }

        [Fact]
        public void BuildClaims_ShouldNotIncludeOptionalClaims_WhenUserPropertiesAreEmpty()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "",
                LastName = "",
                ProfessionalTitle = "",
                Location = "",
                IsAdmin = false,
                IsActive = true
            };

            // Act
            var claims = _service.BuildClaims(user).ToList();

            // Assert
            claims.Should().NotContain(c => c.Type == ClaimTypes.GivenName);
            claims.Should().NotContain(c => c.Type == ClaimTypes.Surname);
            claims.Should().NotContain(c => c.Type == "ProfessionalTitle");
            claims.Should().NotContain(c => c.Type == "Location");
        }

        [Fact]
        public void BuildClaims_ShouldHandleBooleanValues_Correctly()
        {
            // Arrange
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                Username = "admin",
                IsAdmin = true,
                IsActive = false
            };

            // Act
            var claims = _service.BuildClaims(adminUser).ToList();

            // Assert
            claims.Should().Contain(c => c.Type == "IsAdmin" && c.Value == "True");
            claims.Should().Contain(c => c.Type == "IsActive" && c.Value == "False");
        }

        [Fact]
        public void BuildPrincipal_ShouldThrowArgumentNullException_WhenUserIsNull()
        {
            // Act & Assert
            var act = () => _service.BuildPrincipal(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("user");
        }

        [Fact]
        public void BuildPrincipal_ShouldThrowArgumentException_WhenAuthenticationTypeIsNull()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser"
            };

            // Act & Assert
            var act = () => _service.BuildPrincipal(user, null!);
            act.Should().Throw<ArgumentException>().WithParameterName("authenticationType");
        }

        [Fact]
        public void BuildPrincipal_ShouldThrowArgumentException_WhenAuthenticationTypeIsEmpty()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser"
            };

            // Act & Assert
            var act = () => _service.BuildPrincipal(user, "");
            act.Should().Throw<ArgumentException>().WithParameterName("authenticationType");
        }

        [Fact]
        public void BuildPrincipal_ShouldReturnValidPrincipal_WithDefaultAuthenticationType()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                IsAdmin = true,
                IsActive = true
            };

            // Act
            var principal = _service.BuildPrincipal(user);

            // Assert
            principal.Should().NotBeNull();
            principal.Identity.Should().NotBeNull();
            principal.Identity!.AuthenticationType.Should().Be("OAuth2");
            principal.Identity.IsAuthenticated.Should().BeTrue();
            
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(user.Id.ToString());
            principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be(user.Email);
            principal.FindFirst(ClaimTypes.Name)?.Value.Should().Be(user.Username);
            principal.FindFirst("IsAdmin")?.Value.Should().Be("True");
        }

        [Fact]
        public void BuildPrincipal_ShouldReturnValidPrincipal_WithCustomAuthenticationType()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser"
            };
            var customAuthType = "JWT";

            // Act
            var principal = _service.BuildPrincipal(user, customAuthType);

            // Assert
            principal.Should().NotBeNull();
            principal.Identity.Should().NotBeNull();
            principal.Identity!.AuthenticationType.Should().Be(customAuthType);
            principal.Identity.IsAuthenticated.Should().BeTrue();
        }

        [Fact]
        public void BuildPrincipal_ShouldIncludeAllClaimsFromBuildClaims()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "John",
                LastName = "Doe",
                ProfessionalTitle = "Developer",
                Location = "Boston",
                IsAdmin = false,
                IsActive = true,
                LastLoginAt = DateTime.UtcNow
            };

            // Act
            var claims = _service.BuildClaims(user).ToList();
            var principal = _service.BuildPrincipal(user);

            // Assert
            var principalClaims = principal.Claims.ToList();
            principalClaims.Should().HaveCount(claims.Count);
            
            foreach (var claim in claims)
            {
                principalClaims.Should().Contain(c => c.Type == claim.Type && c.Value == claim.Value);
            }
        }

        [Fact]
        public void BuildPrincipal_ShouldCreateClaimsIdentity_WithCorrectProperties()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser"
            };
            var authType = "CustomAuth";

            // Act
            var principal = _service.BuildPrincipal(user, authType);

            // Assert
            var identity = principal.Identity as ClaimsIdentity;
            identity.Should().NotBeNull();
            identity!.AuthenticationType.Should().Be(authType);
            identity.IsAuthenticated.Should().BeTrue();
        }
    }
}
