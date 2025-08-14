using System;
using System.Collections.Generic;
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
    public class AuthenticationContextServiceTests
    {
        private readonly Mock<IAuthenticationStrategy> _mockStrategy1;
        private readonly Mock<IAuthenticationStrategy> _mockStrategy2;
        private readonly Mock<ILogger<AuthenticationContextService>> _mockLogger;
        private readonly AuthenticationContextService _service;
        private readonly DefaultHttpContext _httpContext;

        public AuthenticationContextServiceTests()
        {
            _mockStrategy1 = new Mock<IAuthenticationStrategy>();
            _mockStrategy2 = new Mock<IAuthenticationStrategy>();
            _mockLogger = new Mock<ILogger<AuthenticationContextService>>();
            
            var strategies = new List<IAuthenticationStrategy> { _mockStrategy1.Object, _mockStrategy2.Object };
            _service = new AuthenticationContextService(strategies, _mockLogger.Object);
            
            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Path = "/api/test";
            _httpContext.Request.Method = "POST";
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Arrange & Act
            var strategies = new List<IAuthenticationStrategy> { _mockStrategy1.Object };
            var service = new AuthenticationContextService(strategies, _mockLogger.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullStrategies_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new AuthenticationContextService(null!, _mockLogger.Object);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("strategies");
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Arrange
            var strategies = new List<IAuthenticationStrategy> { _mockStrategy1.Object };

            // Act & Assert
            var action = () => new AuthenticationContextService(strategies, null!);
            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [Fact]
        public async Task AuthenticateAsync_WithNullContext_ShouldReturnNull()
        {
            // Act
            var result = await _service.AuthenticateAsync(null!);

            // Assert
            result.Should().BeNull();
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HttpContext is null")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithNoStrategies_ShouldReturnNull()
        {
            // Arrange
            var emptyStrategies = new List<IAuthenticationStrategy>();
            var service = new AuthenticationContextService(emptyStrategies, _mockLogger.Object);

            // Act
            var result = await service.AuthenticateAsync(_httpContext);

            // Assert
            result.Should().BeNull();
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No authentication strategy could handle the request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithNoMatchingStrategies_ShouldReturnNull()
        {
            // Arrange
            _mockStrategy1.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(false);
            _mockStrategy2.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(false);

            // Act
            var result = await _service.AuthenticateAsync(_httpContext);

            // Assert
            result.Should().BeNull();
            _mockStrategy1.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
            _mockStrategy2.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No authentication strategy could handle the request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithFirstStrategyMatching_ShouldUseFirstStrategy()
        {
            // Arrange
            var expectedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }, "Bearer"));
            
            _mockStrategy1.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _mockStrategy1.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync(expectedPrincipal);
            _mockStrategy2.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(false);

            // Act
            var result = await _service.AuthenticateAsync(_httpContext);

            // Assert
            result.Should().Be(expectedPrincipal);
            _mockStrategy1.Verify(x => x.AuthenticateAsync(_httpContext), Times.Once);
            _mockStrategy2.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Using authentication strategy")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully authenticated using")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithFirstStrategyFailing_ShouldTrySecondStrategy()
        {
            // Arrange
            var expectedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-456") }, "Bearer"));
            
            _mockStrategy1.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _mockStrategy1.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync((ClaimsPrincipal)null!);
            _mockStrategy2.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _mockStrategy2.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync(expectedPrincipal);

            // Act
            var result = await _service.AuthenticateAsync(_httpContext);

            // Assert
            result.Should().Be(expectedPrincipal);
            _mockStrategy1.Verify(x => x.AuthenticateAsync(_httpContext), Times.Once);
            _mockStrategy2.Verify(x => x.AuthenticateAsync(_httpContext), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithFirstStrategyThrowing_ShouldTrySecondStrategy()
        {
            // Arrange
            var expectedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-789") }, "Bearer"));
            var exception = new InvalidOperationException("Strategy 1 failed");
            
            _mockStrategy1.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _mockStrategy1.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ThrowsAsync(exception);
            _mockStrategy2.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _mockStrategy2.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync(expectedPrincipal);

            // Act
            var result = await _service.AuthenticateAsync(_httpContext);

            // Assert
            result.Should().Be(expectedPrincipal);
            _mockStrategy1.Verify(x => x.AuthenticateAsync(_httpContext), Times.Once);
            _mockStrategy2.Verify(x => x.AuthenticateAsync(_httpContext), Times.Once);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Authentication strategy") && v.ToString()!.Contains("failed")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithAllStrategiesFailing_ShouldReturnNull()
        {
            // Arrange
            _mockStrategy1.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _mockStrategy1.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync((ClaimsPrincipal)null!);
            _mockStrategy2.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _mockStrategy2.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync((ClaimsPrincipal)null!);

            // Act
            var result = await _service.AuthenticateAsync(_httpContext);

            // Assert
            result.Should().BeNull();
            _mockStrategy1.Verify(x => x.AuthenticateAsync(_httpContext), Times.Once);
            _mockStrategy2.Verify(x => x.AuthenticateAsync(_httpContext), Times.Once);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No authentication strategy could handle the request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithMixedStrategyResults_ShouldReturnFirstSuccess()
        {
            // Arrange
            var expectedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }, "Bearer"));
            
            _mockStrategy1.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(false);
            _mockStrategy2.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _mockStrategy2.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync(expectedPrincipal);

            // Act
            var result = await _service.AuthenticateAsync(_httpContext);

            // Assert
            result.Should().Be(expectedPrincipal);
            _mockStrategy1.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
            _mockStrategy2.Verify(x => x.AuthenticateAsync(_httpContext), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithStrategyReturningNull_ShouldContinueToNextStrategy()
        {
            // Arrange
            var expectedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-456") }, "Bearer"));
            
            _mockStrategy1.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _mockStrategy1.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync((ClaimsPrincipal)null!);
            _mockStrategy2.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _mockStrategy2.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync(expectedPrincipal);

            // Act
            var result = await _service.AuthenticateAsync(_httpContext);

            // Assert
            result.Should().Be(expectedPrincipal);
            _mockStrategy1.Verify(x => x.AuthenticateAsync(_httpContext), Times.Once);
            _mockStrategy2.Verify(x => x.AuthenticateAsync(_httpContext), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithStrategyThrowingException_ShouldLogErrorAndContinue()
        {
            // Arrange
            var exception = new Exception("Authentication failed");
            _mockStrategy1.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _mockStrategy1.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ThrowsAsync(exception);

            // Act
            var result = await _service.AuthenticateAsync(_httpContext);

            // Assert
            result.Should().BeNull();
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Authentication strategy") && v.ToString()!.Contains("failed")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidContext_ShouldProcessSuccessfully()
        {
            // Arrange
            var expectedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }, "Bearer"));
            _mockStrategy1.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _mockStrategy1.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync(expectedPrincipal);

            // Act
            var result = await _service.AuthenticateAsync(_httpContext);

            // Assert
            result.Should().Be(expectedPrincipal);
            _mockStrategy1.Verify(x => x.CanHandle(_httpContext), Times.Once);
            _mockStrategy1.Verify(x => x.AuthenticateAsync(_httpContext), Times.Once);
        }
    }
}
