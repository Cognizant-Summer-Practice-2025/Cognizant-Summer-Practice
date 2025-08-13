using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using backend_user.Middleware;
using backend_user.Services.Abstractions;
using System.Security.Claims;
using System.IO;
using System.Text;

namespace backend_user.tests.Middleware
{
    public class OAuth2MiddlewareTests
    {
        private readonly Mock<ISecurityHeadersService> _mockSecurityHeadersService;
        private readonly Mock<IAuthorizationPathService> _mockAuthorizationPathService;
        private readonly Mock<IAuthenticationContextService> _mockAuthenticationService;
        private readonly Mock<IClaimsBuilderService> _mockClaimsBuilderService;
        private readonly Mock<ILogger<OAuth2Middleware>> _mockLogger;
        private readonly DefaultHttpContext _context;
        private readonly OAuth2Middleware _middleware;
        private bool _nextCalled;
        private RequestDelegate _next;

        public OAuth2MiddlewareTests()
        {
            _mockSecurityHeadersService = new Mock<ISecurityHeadersService>();
            _mockAuthorizationPathService = new Mock<IAuthorizationPathService>();
            _mockAuthenticationService = new Mock<IAuthenticationContextService>();
            _mockClaimsBuilderService = new Mock<IClaimsBuilderService>();
            _mockLogger = new Mock<ILogger<OAuth2Middleware>>();
            _context = new DefaultHttpContext();
            
            _nextCalled = false;
            _next = ctx => { _nextCalled = true; return Task.CompletedTask; };
            
            _middleware = new OAuth2Middleware(_next, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenNextIsNull()
        {
            // Act & Assert
            var act = () => new OAuth2Middleware(null!, _mockLogger.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("next");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // Act & Assert
            var act = () => new OAuth2Middleware(_next, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task InvokeAsync_ShouldApplySecurityHeaders()
        {
            // Arrange
            _mockAuthorizationPathService.Setup(x => x.RequiresAuthentication(_context)).Returns(false);

            // Act
            await _middleware.InvokeAsync(_context, _mockSecurityHeadersService.Object, 
                _mockAuthorizationPathService.Object, _mockAuthenticationService.Object, 
                _mockClaimsBuilderService.Object);

            // Assert
            _mockSecurityHeadersService.Verify(x => x.ApplySecurityHeaders(_context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_ShouldCallNext_WhenPathDoesNotRequireAuthentication()
        {
            // Arrange
            _mockAuthorizationPathService.Setup(x => x.RequiresAuthentication(_context)).Returns(false);

            // Act
            await _middleware.InvokeAsync(_context, _mockSecurityHeadersService.Object, 
                _mockAuthorizationPathService.Object, _mockAuthenticationService.Object, 
                _mockClaimsBuilderService.Object);

            // Assert
            _nextCalled.Should().BeTrue();
            _mockAuthenticationService.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturn401_WhenAuthenticationFails()
        {
            // Arrange
            _mockAuthorizationPathService.Setup(x => x.RequiresAuthentication(_context)).Returns(true);
            _mockAuthenticationService.Setup(x => x.AuthenticateAsync(_context)).ReturnsAsync((ClaimsPrincipal?)null);
            
            var responseStream = new MemoryStream();
            _context.Response.Body = responseStream;

            // Act
            await _middleware.InvokeAsync(_context, _mockSecurityHeadersService.Object, 
                _mockAuthorizationPathService.Object, _mockAuthenticationService.Object, 
                _mockClaimsBuilderService.Object);

            // Assert
            _context.Response.StatusCode.Should().Be(401);
            _context.Response.ContentType.Should().Be("application/json");
            _nextCalled.Should().BeFalse();

            responseStream.Position = 0;
            var body = new StreamReader(responseStream).ReadToEnd();
            body.Should().Contain("Authentication failed");
        }

        [Fact]
        public async Task InvokeAsync_ShouldSetUserPrincipal_WhenAuthenticationSucceeds()
        {
            // Arrange
            var expectedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Name, "testuser")
            }, "OAuth2"));

            _mockAuthorizationPathService.Setup(x => x.RequiresAuthentication(_context)).Returns(true);
            _mockAuthenticationService.Setup(x => x.AuthenticateAsync(_context)).ReturnsAsync(expectedPrincipal);

            // Act
            await _middleware.InvokeAsync(_context, _mockSecurityHeadersService.Object, 
                _mockAuthorizationPathService.Object, _mockAuthenticationService.Object, 
                _mockClaimsBuilderService.Object);

            // Assert
            _context.User.Should().NotBeNull();
            _context.User.Should().Be(expectedPrincipal);
            _nextCalled.Should().BeTrue();
            _mockClaimsBuilderService.Verify(x => x.BuildPrincipal(It.IsAny<backend_user.Models.User>(), "OAuth2"), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturn401_WhenExceptionOccurs()
        {
            // Arrange
            _mockAuthorizationPathService.Setup(x => x.RequiresAuthentication(_context)).Returns(true);
            _mockAuthenticationService.Setup(x => x.AuthenticateAsync(_context))
                .ThrowsAsync(new Exception("Database connection failed"));
            
            var responseStream = new MemoryStream();
            _context.Response.Body = responseStream;

            // Act
            await _middleware.InvokeAsync(_context, _mockSecurityHeadersService.Object, 
                _mockAuthorizationPathService.Object, _mockAuthenticationService.Object, 
                _mockClaimsBuilderService.Object);

            // Assert
            _context.Response.StatusCode.Should().Be(401);
            _context.Response.ContentType.Should().Be("application/json");
            _nextCalled.Should().BeFalse();

            responseStream.Position = 0;
            var body = new StreamReader(responseStream).ReadToEnd();
            body.Should().Contain("Internal authentication error");
        }

        [Fact]
        public async Task InvokeAsync_ShouldApplySecurityHeaders_EvenForPublicPaths()
        {
            // Arrange
            _context.Request.Path = "/api/users/login";
            _mockAuthorizationPathService.Setup(x => x.RequiresAuthentication(_context)).Returns(false);

            // Act
            await _middleware.InvokeAsync(_context, _mockSecurityHeadersService.Object, 
                _mockAuthorizationPathService.Object, _mockAuthenticationService.Object, 
                _mockClaimsBuilderService.Object);

            // Assert
            _mockSecurityHeadersService.Verify(x => x.ApplySecurityHeaders(_context), Times.Once);
            _mockAuthorizationPathService.Verify(x => x.RequiresAuthentication(_context), Times.Once);
            _mockAuthenticationService.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
            _mockClaimsBuilderService.Verify(x => x.BuildPrincipal(It.IsAny<backend_user.Models.User>(), It.IsAny<string>()), Times.Never);
            
            _nextCalled.Should().BeTrue();
            _context.Response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task InvokeAsync_ShouldFollowCompleteFlow_ForSuccessfulAuthentication()
        {
            // Arrange
            var expectedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }, "OAuth2"));

            _mockAuthorizationPathService.Setup(x => x.RequiresAuthentication(_context)).Returns(true);
            _mockAuthenticationService.Setup(x => x.AuthenticateAsync(_context)).ReturnsAsync(expectedPrincipal);

            // Act
            await _middleware.InvokeAsync(_context, _mockSecurityHeadersService.Object, 
                _mockAuthorizationPathService.Object, _mockAuthenticationService.Object, 
                _mockClaimsBuilderService.Object);

            // Assert
            // Verify all services were called in the correct order
            _mockSecurityHeadersService.Verify(x => x.ApplySecurityHeaders(_context), Times.Once);
            _mockAuthorizationPathService.Verify(x => x.RequiresAuthentication(_context), Times.Once);
            _mockAuthenticationService.Verify(x => x.AuthenticateAsync(_context), Times.Once);
            _mockClaimsBuilderService.Verify(x => x.BuildPrincipal(It.IsAny<backend_user.Models.User>(), "OAuth2"), Times.Never);
            
            _context.User.Should().Be(expectedPrincipal);
            _nextCalled.Should().BeTrue();
            _context.Response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task InvokeAsync_ShouldFollowCompleteFlow_ForPublicPath()
        {
            // Arrange
            _context.Request.Path = "/api/users/login";
            _mockAuthorizationPathService.Setup(x => x.RequiresAuthentication(_context)).Returns(false);

            // Act
            await _middleware.InvokeAsync(_context, _mockSecurityHeadersService.Object, 
                _mockAuthorizationPathService.Object, _mockAuthenticationService.Object, 
                _mockClaimsBuilderService.Object);

            // Assert
            _mockSecurityHeadersService.Verify(x => x.ApplySecurityHeaders(_context), Times.Once);
            _mockAuthorizationPathService.Verify(x => x.RequiresAuthentication(_context), Times.Once);
            _mockAuthenticationService.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
            _mockClaimsBuilderService.Verify(x => x.BuildPrincipal(It.IsAny<backend_user.Models.User>(), It.IsAny<string>()), Times.Never);
            
            _nextCalled.Should().BeTrue();
            _context.Response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturn401WithJsonError_ForFailedAuthentication()
        {
            // Arrange
            _mockAuthorizationPathService.Setup(x => x.RequiresAuthentication(_context)).Returns(true);
            _mockAuthenticationService.Setup(x => x.AuthenticateAsync(_context)).ReturnsAsync((ClaimsPrincipal?)null);
            
            var responseStream = new MemoryStream();
            _context.Response.Body = responseStream;

            // Act
            await _middleware.InvokeAsync(_context, _mockSecurityHeadersService.Object, 
                _mockAuthorizationPathService.Object, _mockAuthenticationService.Object, 
                _mockClaimsBuilderService.Object);

            // Assert
            _context.Response.StatusCode.Should().Be(401);
            _context.Response.ContentType.Should().Be("application/json");
            
            responseStream.Position = 0;
            var body = new StreamReader(responseStream).ReadToEnd();
            body.Should().Be("{\"error\": \"Unauthorized: Authentication failed\"}");
        }

        [Fact]
        public async Task InvokeAsync_ShouldCallAuthorizationPathService_WithCorrectContext()
        {
            // Arrange
            _mockAuthorizationPathService.Setup(x => x.RequiresAuthentication(_context)).Returns(false);

            // Act
            await _middleware.InvokeAsync(_context, _mockSecurityHeadersService.Object, 
                _mockAuthorizationPathService.Object, _mockAuthenticationService.Object, 
                _mockClaimsBuilderService.Object);

            // Assert
            _mockAuthorizationPathService.Verify(x => x.RequiresAuthentication(_context), Times.Once);
        }
    }
}
