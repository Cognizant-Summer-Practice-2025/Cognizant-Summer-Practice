using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using backend_portfolio.Services;
using backend_portfolio.Services.Abstractions;

namespace backend_portfolio.tests.Services
{
    public class OAuth2AuthenticationStrategyTests
    {
        private readonly Mock<IUserAuthenticationService> _mockUserAuthService;
        private readonly Mock<ILogger<OAuth2AuthenticationStrategy>> _mockLogger;
        private readonly OAuth2AuthenticationStrategy _strategy;
        private readonly DefaultHttpContext _httpContext;

        public OAuth2AuthenticationStrategyTests()
        {
            _mockUserAuthService = new Mock<IUserAuthenticationService>();
            _mockLogger = new Mock<ILogger<OAuth2AuthenticationStrategy>>();
            _strategy = new OAuth2AuthenticationStrategy(_mockUserAuthService.Object, _mockLogger.Object);
            
            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Path = "/api/test";
            _httpContext.Request.Method = "POST";
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            var strategy = new OAuth2AuthenticationStrategy(_mockUserAuthService.Object, _mockLogger.Object);
            strategy.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullUserAuthService_ShouldThrowArgumentNullException()
        {
            var action = () => new OAuth2AuthenticationStrategy(null!, _mockLogger.Object);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("userAuthenticationService");
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            var action = () => new OAuth2AuthenticationStrategy(_mockUserAuthService.Object, null!);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [Theory]
        [InlineData("Bearer valid-token")]
        [InlineData("BEARER valid-token")]
        [InlineData("bearer valid-token")]
        [InlineData("Bearer token123")]
        public void CanHandle_WithBearerToken_ShouldReturnTrue(string authHeader)
        {
            _httpContext.Request.Headers["Authorization"] = authHeader;
            var result = _strategy.CanHandle(_httpContext);
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("Basic dXNlcjpwYXNz")]
        [InlineData("Digest username=\"user\"")]
        [InlineData("")]
        [InlineData(null)]
        public void CanHandle_WithNonBearerToken_ShouldReturnFalse(string? authHeader)
        {
            if (authHeader != null)
                _httpContext.Request.Headers["Authorization"] = authHeader;
            
            var result = _strategy.CanHandle(_httpContext);
            result.Should().BeFalse();
        }

        [Fact]
        public void CanHandle_WithNoAuthorizationHeader_ShouldReturnFalse()
        {
            var result = _strategy.CanHandle(_httpContext);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidBearerToken_ShouldReturnUser()
        {
            var expectedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }, "Bearer"));
            _httpContext.Request.Headers["Authorization"] = "Bearer valid-token";
            
            _mockUserAuthService.Setup(x => x.ValidateTokenAsync("valid-token"))
                .ReturnsAsync(expectedPrincipal);

            var result = await _strategy.AuthenticateAsync(_httpContext);

            result.Should().Be(expectedPrincipal);
            _mockUserAuthService.Verify(x => x.ValidateTokenAsync("valid-token"), Times.Once);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully validated token for user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithMissingAuthorizationHeader_ShouldReturnNull()
        {
            var result = await _strategy.AuthenticateAsync(_httpContext);

            result.Should().BeNull();
            _mockUserAuthService.Verify(x => x.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Missing or invalid Authorization header format")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Theory]
        [InlineData("Basic dXNlcjpwYXNz")]
        [InlineData("Digest username=\"user\"")]
        [InlineData("")]
        public async Task AuthenticateAsync_WithNonBearerScheme_ShouldReturnNull(string authHeader)
        {
            _httpContext.Request.Headers["Authorization"] = authHeader;
            
            var result = await _strategy.AuthenticateAsync(_httpContext);

            result.Should().BeNull();
            _mockUserAuthService.Verify(x => x.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Missing or invalid Authorization header format")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Theory]
        [InlineData("Bearer ")]
        [InlineData("Bearer\t")]
        [InlineData("Bearer\n")]
        [InlineData("Bearer    ")]
        public async Task AuthenticateAsync_WithWhitespaceOnlyToken_ShouldReturnNull(string authHeader)
        {
            _httpContext.Request.Headers["Authorization"] = authHeader;
            
            var result = await _strategy.AuthenticateAsync(_httpContext);

            result.Should().BeNull();
            _mockUserAuthService.Verify(x => x.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
            
            // "Bearer " (space only) and "Bearer    " (multiple spaces) get trimmed to empty token
            // "Bearer\t", "Bearer\n" don't start with "Bearer " so they fail format check
            if (authHeader == "Bearer " || authHeader == "Bearer    ")
            {
                _mockLogger.Verify(x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Empty access token provided")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
            }
            else
            {
                _mockLogger.Verify(x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Missing or invalid Authorization header format")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
            }
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidTokenButInvalidUser_ShouldReturnNull()
        {
            _httpContext.Request.Headers["Authorization"] = "Bearer valid-token";
            
            _mockUserAuthService.Setup(x => x.ValidateTokenAsync("valid-token"))
                .ReturnsAsync((ClaimsPrincipal)null!);

            var result = await _strategy.AuthenticateAsync(_httpContext);

            result.Should().BeNull();
            _mockUserAuthService.Verify(x => x.ValidateTokenAsync("valid-token"), Times.Once);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid or expired access token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WhenUserAuthServiceThrows_ShouldReturnNull()
        {
            _httpContext.Request.Headers["Authorization"] = "Bearer valid-token";
            var exception = new Exception("Service error");
            
            _mockUserAuthService.Setup(x => x.ValidateTokenAsync("valid-token"))
                .ThrowsAsync(exception);

            var result = await _strategy.AuthenticateAsync(_httpContext);

            result.Should().BeNull();
            _mockUserAuthService.Verify(x => x.ValidateTokenAsync("valid-token"), Times.Once);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred during OAuth2 authentication")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithCaseInsensitiveBearer_ShouldWork()
        {
            var expectedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }, "Bearer"));
            _httpContext.Request.Headers["Authorization"] = "BEARER valid-token";
            
            _mockUserAuthService.Setup(x => x.ValidateTokenAsync("valid-token"))
                .ReturnsAsync(expectedPrincipal);

            var result = await _strategy.AuthenticateAsync(_httpContext);

            result.Should().Be(expectedPrincipal);
            _mockUserAuthService.Verify(x => x.ValidateTokenAsync("valid-token"), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithWhitespaceAroundToken_ShouldTrimToken()
        {
            var expectedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }, "Bearer"));
            _httpContext.Request.Headers["Authorization"] = "Bearer  valid-token  ";
            
            _mockUserAuthService.Setup(x => x.ValidateTokenAsync("valid-token"))
                .ReturnsAsync(expectedPrincipal);

            var result = await _strategy.AuthenticateAsync(_httpContext);

            result.Should().Be(expectedPrincipal);
            _mockUserAuthService.Verify(x => x.ValidateTokenAsync("valid-token"), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithNullContext_ShouldReturnNull()
        {
            var result = await _strategy.AuthenticateAsync(null!);
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidTokenAndUser_ShouldLogSuccess()
        {
            var expectedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }, "Bearer"));
            _httpContext.Request.Headers["Authorization"] = "Bearer valid-token";
            
            _mockUserAuthService.Setup(x => x.ValidateTokenAsync("valid-token"))
                .ReturnsAsync(expectedPrincipal);

            await _strategy.AuthenticateAsync(_httpContext);

            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully validated token for user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
