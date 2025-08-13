using System.Security.Claims;
using backend_AI.Services;
using backend_AI.Services.Abstractions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend_AI.tests.Services
{
    public class OAuth2AuthenticationStrategyTests
    {
        private readonly Mock<IUserAuthenticationService> _userAuth = new();
        private readonly Mock<ILogger<OAuth2AuthenticationStrategy>> _logger = new();
        private readonly OAuth2AuthenticationStrategy _strategy;
        private readonly DefaultHttpContext _ctx = new();

        public OAuth2AuthenticationStrategyTests()
        {
            _strategy = new OAuth2AuthenticationStrategy(_userAuth.Object, _logger.Object);
        }

        [Theory]
        [InlineData("Bearer token", true)]
        [InlineData("bearer token", true)]
        [InlineData("Basic abc", false)]
        [InlineData(null, false)]
        public void CanHandle_Works(string? header, bool expected)
        {
            if (header != null) _ctx.Request.Headers["Authorization"] = header;
            _strategy.CanHandle(_ctx).Should().Be(expected);
        }

        [Fact]
        public async Task Authenticate_MissingHeader_ReturnsNull()
        {
            var result = await _strategy.AuthenticateAsync(_ctx);
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("Basic abc")]
        [InlineData("")]
        public async Task Authenticate_InvalidScheme_ReturnsNull(string header)
        {
            _ctx.Request.Headers["Authorization"] = header;
            var result = await _strategy.AuthenticateAsync(_ctx);
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("Bearer ")]
        [InlineData("Bearer    ")]
        public async Task Authenticate_EmptyToken_ReturnsNull(string header)
        {
            _ctx.Request.Headers["Authorization"] = header;
            var result = await _strategy.AuthenticateAsync(_ctx);
            result.Should().BeNull();
            _userAuth.Verify(u => u.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Authenticate_InvalidToken_ReturnsNull()
        {
            _ctx.Request.Headers["Authorization"] = "Bearer token";
            _userAuth.Setup(u => u.ValidateTokenAsync("token")).ReturnsAsync((ClaimsPrincipal?)null);
            var result = await _strategy.AuthenticateAsync(_ctx);
            result.Should().BeNull();
        }

        [Fact]
        public async Task Authenticate_Exception_ReturnsNull()
        {
            _ctx.Request.Headers["Authorization"] = "Bearer token";
            _userAuth.Setup(u => u.ValidateTokenAsync("token")).ThrowsAsync(new InvalidOperationException("boom"));
            var result = await _strategy.AuthenticateAsync(_ctx);
            result.Should().BeNull();
        }

        [Fact]
        public async Task Authenticate_Success_ReturnsPrincipal()
        {
            _ctx.Request.Headers["Authorization"] = "Bearer token";
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "u") }, "OAuth2"));
            _userAuth.Setup(u => u.ValidateTokenAsync("token")).ReturnsAsync(principal);
            var result = await _strategy.AuthenticateAsync(_ctx);
            result.Should().Be(principal);
        }
    }
}
