using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using backend_user.Controllers;
using backend_user.Services.Abstractions;
using backend_user.DTO.OAuth.Request;
using backend_user.DTO.OAuth.Response;
using backend_user.Models;
using backend_user.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace backend_user.tests.Controllers
{
    public class OAuth2ControllerTests
    {
        private readonly Mock<IOAuth2Service> _mockOAuth2Service;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IOAuthProviderRepository> _mockOAuthProviderRepository;
        private readonly OAuth2Controller _controller;

        public OAuth2ControllerTests()
        {
            _mockOAuth2Service = new Mock<IOAuth2Service>();
            _mockUserService = new Mock<IUserService>();
            _mockOAuthProviderRepository = new Mock<IOAuthProviderRepository>();
            _controller = new OAuth2Controller(
                _mockOAuth2Service.Object,
                _mockUserService.Object,
                _mockOAuthProviderRepository.Object
            );
        }

        [Fact]
        public async Task ValidateToken_WithValidToken_ShouldReturnOkWithUser()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Username = "testuser" };
            var request = new ValidateTokenRequest { AccessToken = "valid-token" };
            _mockOAuth2Service.Setup(x => x.GetUserByAccessTokenAsync(request.AccessToken)).ReturnsAsync(user);

            var result = await _controller.ValidateToken(request);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ValidateTokenResponse>().Subject;
            response.UserId.Should().Be(user.Id);
            response.Email.Should().Be(user.Email);
            response.Username.Should().Be(user.Username);
        }

        [Fact]
        public async Task ValidateToken_WithInvalidToken_ShouldReturnUnauthorized()
        {
            var request = new ValidateTokenRequest { AccessToken = "invalid-token" };
            _mockOAuth2Service.Setup(x => x.GetUserByAccessTokenAsync(request.AccessToken)).ReturnsAsync((User)null);

            var result = await _controller.ValidateToken(request);

            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorized.Value.Should().BeEquivalentTo(new { message = "Invalid or expired access token" });
        }

        [Fact]
        public async Task ValidateToken_WhenServiceThrowsException_ShouldReturnBadRequest()
        {
            var request = new ValidateTokenRequest { AccessToken = "error-token" };
            _mockOAuth2Service.Setup(x => x.GetUserByAccessTokenAsync(request.AccessToken)).ThrowsAsync(new Exception("Service error"));

            var result = await _controller.ValidateToken(request);

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().BeEquivalentTo(new { message = "Service error" });
        }

        [Fact]
        public async Task RefreshToken_WithValidRefreshToken_ShouldReturnOkWithNewToken()
        {
            var request = new RefreshTokenRequest { RefreshToken = "refresh-token" };
            var provider = new OAuthProvider
            {
                AccessToken = "new-access-token",
                TokenExpiresAt = DateTime.UtcNow.AddHours(1),
                RefreshToken = "refresh-token"
            };
            _mockOAuth2Service.Setup(x => x.RefreshAccessTokenAsync(request.RefreshToken)).ReturnsAsync(provider);

            var result = await _controller.RefreshToken(request);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var json = JsonSerializer.Serialize(okResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            root.GetProperty("message").GetString().Should().Be("Token refreshed successfully");
            root.GetProperty("accessToken").GetString().Should().Be(provider.AccessToken);
        }

        [Fact]
        public async Task RefreshToken_WithInvalidRefreshToken_ShouldReturnBadRequest()
        {
            var request = new RefreshTokenRequest { RefreshToken = "invalid-refresh" };
            _mockOAuth2Service.Setup(x => x.RefreshAccessTokenAsync(request.RefreshToken)).ReturnsAsync((OAuthProvider)null);

            var result = await _controller.RefreshToken(request);

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var json = JsonSerializer.Serialize(badRequest.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            root.GetProperty("message").GetString().Should().Contain("Token refresh failed");
        }

        [Fact]
        public async Task RefreshToken_WhenServiceThrowsException_ShouldReturnBadRequest()
        {
            var request = new RefreshTokenRequest { RefreshToken = "error-refresh" };
            _mockOAuth2Service.Setup(x => x.RefreshAccessTokenAsync(request.RefreshToken)).ThrowsAsync(new Exception("Refresh error"));

            var result = await _controller.RefreshToken(request);

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().BeEquivalentTo(new { message = "Refresh error" });
        }

        [Fact]
        public async Task GetTokenStatus_WithValidToken_ShouldReturnOkWithStatus()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };
            var providers = new List<OAuthProvider>
            {
                new OAuthProvider { Provider = OAuthProviderType.Google, RefreshToken = "refresh", TokenExpiresAt = DateTime.UtcNow.AddHours(1) },
                new OAuthProvider { Provider = OAuthProviderType.GitHub, RefreshToken = null, TokenExpiresAt = DateTime.UtcNow.AddHours(-1) }
            };
            var token = "valid-token";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _mockOAuth2Service.Setup(x => x.GetUserByAccessTokenAsync(token)).ReturnsAsync(user);
            _mockOAuthProviderRepository.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(providers);

            var result = await _controller.GetTokenStatus();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var json = JsonSerializer.Serialize(okResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            root.GetProperty("UserId").GetGuid().Should().Be(user.Id);
            root.GetProperty("UserEmail").GetString().Should().Be(user.Email);
        }

        [Fact]
        public async Task GetTokenStatus_WithMissingAuthorizationHeader_ShouldReturnUnauthorized()
        {
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await _controller.GetTokenStatus();

            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorized.Value.Should().BeEquivalentTo(new { message = "Missing or invalid Authorization header" });
        }

        [Fact]
        public async Task GetTokenStatus_WithInvalidToken_ShouldReturnUnauthorized()
        {
            var token = "invalid-token";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _mockOAuth2Service.Setup(x => x.GetUserByAccessTokenAsync(token)).ReturnsAsync((User)null);

            var result = await _controller.GetTokenStatus();

            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorized.Value.Should().BeEquivalentTo(new { message = "Invalid access token" });
        }

        [Fact]
        public async Task GetTokenStatus_WhenServiceThrowsException_ShouldReturnBadRequest()
        {
            var token = "error-token";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _mockOAuth2Service.Setup(x => x.GetUserByAccessTokenAsync(token)).ThrowsAsync(new Exception("Token status error"));

            var result = await _controller.GetTokenStatus();

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().BeEquivalentTo(new { message = "Token status error" });
        }

        [Fact]
        public async Task GetCurrentUser_WithValidToken_ShouldReturnOkWithUser()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "John",
                LastName = "Doe",
                ProfessionalTitle = "Engineer",
                Bio = "Bio",
                Location = "Location",
                AvatarUrl = "avatar.png",
                IsActive = true,
                IsAdmin = false,
                LastLoginAt = DateTime.UtcNow
            };
            var token = "valid-token";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _mockOAuth2Service.Setup(x => x.GetUserByAccessTokenAsync(token)).ReturnsAsync(user);

            var result = await _controller.GetCurrentUser();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ValidateTokenResponse>().Subject;
            response.UserId.Should().Be(user.Id);
            response.Email.Should().Be(user.Email);
            response.Username.Should().Be(user.Username);
            response.FirstName.Should().Be(user.FirstName);
            response.LastName.Should().Be(user.LastName);
            response.ProfessionalTitle.Should().Be(user.ProfessionalTitle);
            response.Bio.Should().Be(user.Bio);
            response.Location.Should().Be(user.Location);
            response.AvatarUrl.Should().Be(user.AvatarUrl);
            response.IsActive.Should().Be(user.IsActive);
            response.IsAdmin.Should().Be(user.IsAdmin);
            response.LastLoginAt.Should().Be(user.LastLoginAt);
        }

        [Fact]
        public async Task GetCurrentUser_WithMissingAuthorizationHeader_ShouldReturnUnauthorized()
        {
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await _controller.GetCurrentUser();

            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorized.Value.Should().BeEquivalentTo(new { message = "Missing or invalid Authorization header" });
        }

        [Fact]
        public async Task GetCurrentUser_WithInvalidToken_ShouldReturnUnauthorized()
        {
            var token = "invalid-token";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _mockOAuth2Service.Setup(x => x.GetUserByAccessTokenAsync(token)).ReturnsAsync((User)null);

            var result = await _controller.GetCurrentUser();

            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorized.Value.Should().BeEquivalentTo(new { message = "Invalid or expired access token" });
        }

        [Fact]
        public async Task GetCurrentUser_WhenServiceThrowsException_ShouldReturnBadRequest()
        {
            var token = "error-token";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _mockOAuth2Service.Setup(x => x.GetUserByAccessTokenAsync(token)).ThrowsAsync(new Exception("Get user error"));

            var result = await _controller.GetCurrentUser();

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().BeEquivalentTo(new { message = "Get user error" });
        }
    }
}