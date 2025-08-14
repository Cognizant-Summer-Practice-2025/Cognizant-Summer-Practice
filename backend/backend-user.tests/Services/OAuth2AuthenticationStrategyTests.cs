using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using backend_user.Services;
using backend_user.Services.Abstractions;
using System.Security.Claims;

namespace backend_user.tests.Services
{
    public class OAuth2AuthenticationStrategyTests
    {
        private readonly Mock<IOAuth2Service> _mockOAuth2Service;
        private readonly Mock<ILogger<OAuth2AuthenticationStrategy>> _mockLogger;
        private readonly OAuth2AuthenticationStrategy _strategy;
        private readonly DefaultHttpContext _context;

        public OAuth2AuthenticationStrategyTests()
        {
            _mockOAuth2Service = new Mock<IOAuth2Service>();
            _mockLogger = new Mock<ILogger<OAuth2AuthenticationStrategy>>();
            _strategy = new OAuth2AuthenticationStrategy(_mockOAuth2Service.Object, _mockLogger.Object);
            _context = new DefaultHttpContext();
        }

        [Theory]
        [InlineData("Bearer valid-token")]
        [InlineData("bearer valid-token")]
        [InlineData("BEARER valid-token")]
        public void CanHandle_ShouldReturnTrue_WhenAuthorizationHeaderHasBearerToken(string authHeader)
        {
            // Arrange
            _context.Request.Headers.Authorization = authHeader;

            // Act
            var result = _strategy.CanHandle(_context);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("Basic xyz123")]
        [InlineData("Digest xyz123")]
        [InlineData("Custom xyz123")]
        [InlineData("Bearer")]
        public void CanHandle_ShouldReturnFalse_WhenAuthorizationHeaderIsNotBearerToken(string authHeader)
        {
            // Arrange
            _context.Request.Headers.Authorization = authHeader;

            // Act
            var result = _strategy.CanHandle(_context);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanHandle_ShouldReturnFalse_WhenNoAuthorizationHeader()
        {
            // Arrange
            _context.Request.Headers.Remove("Authorization");

            // Act
            var result = _strategy.CanHandle(_context);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenNoAuthorizationHeader()
        {
            // Arrange
            _context.Request.Headers.Remove("Authorization");

            // Act
            var result = await _strategy.AuthenticateAsync(_context);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenAuthorizationHeaderIsNotBearer()
        {
            // Arrange
            _context.Request.Headers.Authorization = "Basic xyz123";

            // Act
            var result = await _strategy.AuthenticateAsync(_context);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenTokenIsEmpty()
        {
            // Arrange
            _context.Request.Headers.Authorization = "Bearer ";

            // Act
            var result = await _strategy.AuthenticateAsync(_context);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenTokenIsOnlyWhitespace()
        {
            // Arrange
            _context.Request.Headers.Authorization = "Bearer   ";

            // Act
            var result = await _strategy.AuthenticateAsync(_context);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenOAuth2ServiceReturnsNull()
        {
            // Arrange
            _context.Request.Headers.Authorization = "Bearer invalid-token";
            _mockOAuth2Service.Setup(x => x.GetUserByAccessTokenAsync("invalid-token"))
                .ReturnsAsync((backend_user.Models.User?)null);

            // Act
            var result = await _strategy.AuthenticateAsync(_context);

            // Assert
            result.Should().BeNull();
            _mockOAuth2Service.Verify(x => x.GetUserByAccessTokenAsync("invalid-token"), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnPrincipal_WhenTokenIsValid()
        {
            // Arrange
            var validToken = "valid-token-123";
            var expectedUser = new backend_user.Models.User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                IsAdmin = false
            };

            _context.Request.Headers.Authorization = $"Bearer {validToken}";
            _mockOAuth2Service.Setup(x => x.GetUserByAccessTokenAsync(validToken))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _strategy.AuthenticateAsync(_context);

            // Assert
            result.Should().NotBeNull();
            result!.Identity!.IsAuthenticated.Should().BeTrue();
            result!.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be(expectedUser.Id.ToString());
            _mockOAuth2Service.Verify(x => x.GetUserByAccessTokenAsync(validToken), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenOAuth2ServiceThrowsException()
        {
            // Arrange
            var token = "problematic-token";
            _context.Request.Headers.Authorization = $"Bearer {token}";
            _mockOAuth2Service.Setup(x => x.GetUserByAccessTokenAsync(token))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _strategy.AuthenticateAsync(_context);

            // Assert
            result.Should().BeNull();
            _mockOAuth2Service.Verify(x => x.GetUserByAccessTokenAsync(token), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldTrimTokenWhitespace()
        {
            // Arrange
            var token = "token-with-spaces";
            var expectedUser = new backend_user.Models.User { Id = Guid.NewGuid(), Email = "test@example.com", Username = "testuser" };
            
            _context.Request.Headers.Authorization = $"Bearer   {token}   ";
            _mockOAuth2Service.Setup(x => x.GetUserByAccessTokenAsync(token))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _strategy.AuthenticateAsync(_context);

            // Assert
            result.Should().NotBeNull();
            _mockOAuth2Service.Verify(x => x.GetUserByAccessTokenAsync(token), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenOAuth2ServiceIsNull()
        {
            // Act & Assert
            var act = () => new OAuth2AuthenticationStrategy(null!, _mockLogger.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("oauth2Service");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // Act & Assert
            var act = () => new OAuth2AuthenticationStrategy(_mockOAuth2Service.Object, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }
    }
}
