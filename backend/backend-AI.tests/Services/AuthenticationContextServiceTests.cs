using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using backend_AI.Services;
using backend_AI.Services.Abstractions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend_AI.tests.Services
{
    public class AuthenticationContextServiceTests
    {
        private readonly Mock<IAuthenticationStrategy> _s1 = new();
        private readonly Mock<IAuthenticationStrategy> _s2 = new();
        private readonly Mock<ILogger<AuthenticationContextService>> _logger = new();
        private readonly DefaultHttpContext _ctx = new();

        private AuthenticationContextService CreateService(params IAuthenticationStrategy[] strategies)
        {
            return new AuthenticationContextService(strategies, _logger.Object);
        }

        [Fact]
        public void Ctor_NullStrategies_Throws()
        {
            Action act = () => new AuthenticationContextService(null!, _logger.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("strategies");
        }

        [Fact]
        public void Ctor_NullLogger_Throws()
        {
            Action act = () => new AuthenticationContextService(new[] { _s1.Object }, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task AuthenticateAsync_NullContext_ReturnsNull()
        {
            var service = CreateService(_s1.Object);
            var result = await service.AuthenticateAsync(null!);
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_NoMatchingStrategy_ReturnsNull()
        {
            _s1.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(false);
            _s2.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(false);
            var service = CreateService(_s1.Object, _s2.Object);

            var result = await service.AuthenticateAsync(_ctx);

            result.Should().BeNull();
            _s1.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
            _s2.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_FirstMatching_Succeeds()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity("test"));
            _s1.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _s1.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync(principal);
            _s2.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(false);

            var service = CreateService(_s1.Object, _s2.Object);
            var result = await service.AuthenticateAsync(_ctx);

            result.Should().Be(principal);
        }

        [Fact]
        public async Task AuthenticateAsync_FirstReturnsNull_TriesSecond()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity("test"));
            _s1.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _s1.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync((ClaimsPrincipal?)null);
            _s2.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _s2.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync(principal);

            var service = CreateService(_s1.Object, _s2.Object);
            var result = await service.AuthenticateAsync(_ctx);

            result.Should().Be(principal);
        }

        [Fact]
        public async Task AuthenticateAsync_FirstThrows_TriesSecond()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity("test"));
            _s1.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _s1.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ThrowsAsync(new InvalidOperationException("boom"));
            _s2.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _s2.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync(principal);

            var service = CreateService(_s1.Object, _s2.Object);
            var result = await service.AuthenticateAsync(_ctx);

            result.Should().Be(principal);
        }

        [Fact]
        public async Task AuthenticateAsync_AllFail_ReturnsNull()
        {
            _s1.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _s1.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync((ClaimsPrincipal?)null);
            _s2.Setup(x => x.CanHandle(It.IsAny<HttpContext>())).Returns(true);
            _s2.Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>())).ReturnsAsync((ClaimsPrincipal?)null);

            var service = CreateService(_s1.Object, _s2.Object);
            var result = await service.AuthenticateAsync(_ctx);

            result.Should().BeNull();
        }
    }
}
