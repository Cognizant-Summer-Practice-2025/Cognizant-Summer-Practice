using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using backend_portfolio.Middleware;
using backend_portfolio.Services.Abstractions;

namespace backend_portfolio.tests
{
    public class OAuth2MiddlewareTests
    {
        private readonly Mock<ILogger<OAuth2Middleware>> _mockLogger;
        private readonly DefaultHttpContext _context = new();
        private bool _nextCalled;
        private RequestDelegate _next;

        public OAuth2MiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<OAuth2Middleware>>();
            _nextCalled = false;
            _next = ctx => { _nextCalled = true; return Task.CompletedTask; };
        }

        private OAuth2Middleware CreateMiddleware() => new OAuth2Middleware(_next, _mockLogger.Object);

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenNextIsNull()
        {
            // Act
            var middleware = new OAuth2Middleware(null!, _mockLogger.Object);

            // Assert
            middleware.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenLoggerIsNull()
        {
            // Act
            var middleware = new OAuth2Middleware(_next, null!);

            // Assert
            middleware.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllParametersAreValid()
        {
            // Act
            var middleware = new OAuth2Middleware(_next, _mockLogger.Object);

            // Assert
            middleware.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldSkipAuth_ForSwaggerPaths()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var userAuthService = new Mock<IUserAuthenticationService>();
            _context.Request.Path = "/swagger";
            _context.Request.Method = "GET";

            // Act
            await middleware.InvokeAsync(_context, userAuthService.Object);

            // Assert
            _nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldSkipAuth_ForHealthChecks()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var userAuthService = new Mock<IUserAuthenticationService>();
            _context.Request.Path = "/health";
            _context.Request.Method = "GET";

            // Act
            await middleware.InvokeAsync(_context, userAuthService.Object);

            // Assert
            _nextCalled.Should().BeTrue();
        }
    }
} 