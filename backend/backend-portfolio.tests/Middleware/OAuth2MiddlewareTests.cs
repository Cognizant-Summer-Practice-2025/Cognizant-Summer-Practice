using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using backend_portfolio.Middleware;
using backend_portfolio.Services.Abstractions;

namespace backend_portfolio.tests.Middleware
{
    public class OAuth2MiddlewareTests
    {
        private readonly Mock<RequestDelegate> _mockNext;
        private readonly Mock<ILogger<OAuth2Middleware>> _mockLogger;
        private readonly Mock<ISecurityHeadersService> _securityHeaders;
        private readonly Mock<IAuthorizationPathService> _authorizationPathService;
        private readonly Mock<IAuthenticationContextService> _authenticationContextService;
        private readonly OAuth2Middleware _middleware;

        public OAuth2MiddlewareTests()
        {
            _mockNext = new Mock<RequestDelegate>();
            _mockLogger = new Mock<ILogger<OAuth2Middleware>>();
            _securityHeaders = new Mock<ISecurityHeadersService>();
            _authorizationPathService = new Mock<IAuthorizationPathService>();
            _authenticationContextService = new Mock<IAuthenticationContextService>();
            _middleware = new OAuth2Middleware(_mockNext.Object, _mockLogger.Object);
        }

        private static HttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/test";
            context.Request.Method = "POST";
            return context;
        }

        [Fact]
        public async Task InvokeAsync_WithValidPrincipal_ShouldCallNext()
        {
            var context = CreateHttpContext();
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }, "Bearer"));
            _authorizationPathService.Setup(x => x.RequiresAuthentication(It.IsAny<HttpContext>())).Returns(true);
            _authenticationContextService.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync(principal);
            _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            await _middleware.InvokeAsync(context, _securityHeaders.Object, _authorizationPathService.Object, _authenticationContextService.Object);

            _mockNext.Verify(x => x(context), Times.Once);
            context.User.Should().Be(principal);
            _securityHeaders.Verify(x => x.ApplySecurityHeaders(context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithAuthRequiredAndAuthFails_ShouldReturn401()
        {
            var context = CreateHttpContext();
            _authorizationPathService.Setup(x => x.RequiresAuthentication(It.IsAny<HttpContext>())).Returns(true);
            _authenticationContextService.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync((ClaimsPrincipal)null!);

            await _middleware.InvokeAsync(context, _securityHeaders.Object, _authorizationPathService.Object, _authenticationContextService.Object);

            context.Response.StatusCode.Should().Be(401);
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithAuthNotRequired_ShouldSkipAuth()
        {
            var context = CreateHttpContext();
            _authorizationPathService.Setup(x => x.RequiresAuthentication(It.IsAny<HttpContext>())).Returns(false);
            _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            await _middleware.InvokeAsync(context, _securityHeaders.Object, _authorizationPathService.Object, _authenticationContextService.Object);

            _mockNext.Verify(x => x(context), Times.Once);
            _authenticationContextService.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
            _securityHeaders.Verify(x => x.ApplySecurityHeaders(context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_AuthContextThrows_ShouldReturn401()
        {
            var context = CreateHttpContext();
            _authorizationPathService.Setup(x => x.RequiresAuthentication(It.IsAny<HttpContext>())).Returns(true);
            _authenticationContextService.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ThrowsAsync(new Exception("auth error"));

            await _middleware.InvokeAsync(context, _securityHeaders.Object, _authorizationPathService.Object, _authenticationContextService.Object);

            context.Response.StatusCode.Should().Be(401);
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_NextThrows_ShouldReturn401()
        {
            var context = CreateHttpContext();
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }, "Bearer"));
            _authorizationPathService.Setup(x => x.RequiresAuthentication(It.IsAny<HttpContext>())).Returns(true);
            _authenticationContextService.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync(principal);
            _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(new InvalidOperationException("Next middleware error"));

            await _middleware.InvokeAsync(context, _securityHeaders.Object, _authorizationPathService.Object, _authenticationContextService.Object);

            context.Response.StatusCode.Should().Be(401);
        }

        [Theory]
        [InlineData("/openapi")]
        [InlineData("/swagger")]
        [InlineData("/")]
        [InlineData("/health")]
        [InlineData("/api/portfolio")]
        [InlineData("/api/portfoliotemplate")]
        [InlineData("/api/project")]
        [InlineData("/api/bookmark")]
        [InlineData("/api/image")]
        public async Task InvokeAsync_PublicEndpoints_ShouldSkipAuth(string path)
        {
            var context = CreateHttpContext();
            context.Request.Path = path;
            context.Request.Method = "GET";
            _authorizationPathService.Setup(x => x.RequiresAuthentication(It.IsAny<HttpContext>())).Returns(false);
            _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            await _middleware.InvokeAsync(context, _securityHeaders.Object, _authorizationPathService.Object, _authenticationContextService.Object);

            _mockNext.Verify(x => x(context), Times.Once);
            _authenticationContextService.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
        }

        [Theory]
        [InlineData("/api/portfolio/123/view", "POST")]
        [InlineData("/api/portfoliotemplate/seed", "POST")]
        public async Task InvokeAsync_SpecificPublicPosts_ShouldSkipAuth(string path, string method)
        {
            var context = CreateHttpContext();
            context.Request.Path = path;
            context.Request.Method = method;
            _authorizationPathService.Setup(x => x.RequiresAuthentication(It.IsAny<HttpContext>())).Returns(false);
            _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            await _middleware.InvokeAsync(context, _securityHeaders.Object, _authorizationPathService.Object, _authenticationContextService.Object);

            _mockNext.Verify(x => x(context), Times.Once);
            _authenticationContextService.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
        }
    }
} 