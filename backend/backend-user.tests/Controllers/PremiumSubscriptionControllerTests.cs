using Xunit;
using Moq;
using FluentAssertions;
using backend_user.Controllers;
using backend_user.Services;
using backend_user.Repositories;
using backend_user.DTO.PremiumSubscription;
using backend_user.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace backend_user.tests.Controllers
{
    public class PremiumSubscriptionControllerTests
    {
        private readonly Mock<IStripeService> _mockStripeService;
        private readonly Mock<IPremiumSubscriptionRepository> _mockRepository;
        private readonly Mock<ILogger<PremiumSubscriptionController>> _mockLogger;
        private readonly PremiumSubscriptionController _controller;
        private readonly HttpContext _httpContext;

        public PremiumSubscriptionControllerTests()
        {
            _mockStripeService = new Mock<IStripeService>();
            _mockRepository = new Mock<IPremiumSubscriptionRepository>();
            _mockLogger = new Mock<ILogger<PremiumSubscriptionController>>();
            
            _controller = new PremiumSubscriptionController(
                _mockStripeService.Object,
                _mockRepository.Object,
                _mockLogger.Object);

            // Set up HTTP context with authenticated user
            _httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Name, "Test User")
            };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);
            _httpContext.User = principal;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullStripeService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new PremiumSubscriptionController(null!, _mockRepository.Object, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new PremiumSubscriptionController(_mockStripeService.Object, null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new PremiumSubscriptionController(_mockStripeService.Object, _mockRepository.Object, null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act
            var controller = new PremiumSubscriptionController(
                _mockStripeService.Object,
                _mockRepository.Object,
                _mockLogger.Object);

            // Assert
            controller.Should().NotBeNull();
        }

        #endregion

        #region GetSubscriptionStatus Tests

        [Fact]
        public async Task GetSubscriptionStatus_WithValidUser_ShouldReturnPremiumStatus()
        {
            // Arrange
            var userId = Guid.Parse(_httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            _mockRepository
                .Setup(x => x.IsActiveAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.GetSubscriptionStatus();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { IsPremium = true });
            _mockRepository.Verify(x => x.IsActiveAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetSubscriptionStatus_WithNonPremiumUser_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.Parse(_httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            _mockRepository
                .Setup(x => x.IsActiveAsync(userId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.GetSubscriptionStatus();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { IsPremium = false });
            _mockRepository.Verify(x => x.IsActiveAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetSubscriptionStatus_WithNoUserClaim_ShouldReturnUnauthorized()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Name, "Test User")
            };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);
            _httpContext.User = principal;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };

            // Act
            var result = await _controller.GetSubscriptionStatus();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        #endregion

        #region CreateCheckoutSession Tests

        [Fact]
        public async Task CreateCheckoutSession_WithValidRequest_ShouldReturnCheckoutUrl()
        {
            // Arrange
            var userId = Guid.Parse(_httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var request = new CreateCheckoutSessionRequest
            {
                SuccessUrl = "https://example.com/success",
                CancelUrl = "https://example.com/cancel"
            };
            var expectedUrl = "https://checkout.stripe.com/test";

            _mockStripeService
                .Setup(x => x.CreateCheckoutSessionAsync(userId, request.SuccessUrl, request.CancelUrl))
                .ReturnsAsync(expectedUrl);

            // Act
            var result = await _controller.CreateCheckoutSession(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { CheckoutUrl = expectedUrl });
            _mockStripeService.Verify(x => x.CreateCheckoutSessionAsync(userId, request.SuccessUrl, request.CancelUrl), Times.Once);
        }

        [Fact]
        public async Task CreateCheckoutSession_WithNoUserClaim_ShouldReturnUnauthorized()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Name, "Test User")
            };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);
            _httpContext.User = principal;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };

            var request = new CreateCheckoutSessionRequest
            {
                SuccessUrl = "https://example.com/success",
                CancelUrl = "https://example.com/cancel"
            };

            // Act
            var result = await _controller.CreateCheckoutSession(request);

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task CreateCheckoutSession_WithStripeServiceException_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = Guid.Parse(_httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var request = new CreateCheckoutSessionRequest
            {
                SuccessUrl = "https://example.com/success",
                CancelUrl = "https://example.com/cancel"
            };

            _mockStripeService
                .Setup(x => x.CreateCheckoutSessionAsync(userId, request.SuccessUrl, request.CancelUrl))
                .ThrowsAsync(new Exception("Stripe error"));

            // Act
            var result = await _controller.CreateCheckoutSession(request);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { Error = "Stripe error" });
        }

        #endregion

        #region HandleWebhook Tests

        [Fact]
        public async Task HandleWebhook_WithValidWebhook_ShouldReturnOk()
        {
            // Arrange
            var webhookJson = "{\"test\": \"data\"}";
            var signature = "valid_signature";

            _mockStripeService
                .Setup(x => x.HandleWebhookAsync(webhookJson, signature))
                .ReturnsAsync(true);

            // Set up request body
            var request = _controller.Request;
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(webhookJson));
            request.Headers["Stripe-Signature"] = signature;

            // Act
            var result = await _controller.HandleWebhook();

            // Assert
            result.Should().BeOfType<OkResult>();
            _mockStripeService.Verify(x => x.HandleWebhookAsync(webhookJson, signature), Times.Once);
        }

        [Fact]
        public async Task HandleWebhook_WithMissingSignature_ShouldReturnBadRequest()
        {
            // Arrange
            var webhookJson = "{\"test\": \"data\"}";

            // Set up request body without signature header
            var request = _controller.Request;
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(webhookJson));

            // Act
            var result = await _controller.HandleWebhook();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task HandleWebhook_WithEmptySignature_ShouldReturnBadRequest()
        {
            // Arrange
            var webhookJson = "{\"test\": \"data\"}";

            // Set up request body with empty signature header
            var request = _controller.Request;
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(webhookJson));
            request.Headers["Stripe-Signature"] = "";

            // Act
            var result = await _controller.HandleWebhook();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task HandleWebhook_WithStripeServiceFailure_ShouldReturnBadRequest()
        {
            // Arrange
            var webhookJson = "{\"test\": \"data\"}";
            var signature = "valid_signature";

            _mockStripeService
                .Setup(x => x.HandleWebhookAsync(webhookJson, signature))
                .ReturnsAsync(false);

            // Set up request body
            var request = _controller.Request;
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(webhookJson));
            request.Headers["Stripe-Signature"] = signature;

            // Act
            var result = await _controller.HandleWebhook();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task HandleWebhook_WithException_ShouldReturnBadRequest()
        {
            // Arrange
            var webhookJson = "{\"test\": \"data\"}";
            var signature = "valid_signature";

            _mockStripeService
                .Setup(x => x.HandleWebhookAsync(webhookJson, signature))
                .ThrowsAsync(new Exception("Webhook error"));

            // Set up request body
            var request = _controller.Request;
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(webhookJson));
            request.Headers["Stripe-Signature"] = signature;

            // Act
            var result = await _controller.HandleWebhook();

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var response = badRequestResult.Value.Should().BeOfType<string>().Subject;
            response.Should().Contain("Webhook error");
        }

        #endregion

        #region CancelSubscription Tests

        [Fact]
        public async Task CancelSubscription_WithValidUser_ShouldReturnOk()
        {
            // Arrange
            var userId = Guid.Parse(_httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var subscription = new PremiumSubscription
            {
                Id = Guid.NewGuid(),
                StripeSubscriptionId = "sub_test123"
            };

            _mockRepository
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(subscription);

            _mockStripeService
                .Setup(x => x.CancelSubscriptionAsync(subscription.StripeSubscriptionId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CancelSubscription();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { Message = "Subscription cancelled successfully" });
            _mockRepository.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
            _mockStripeService.Verify(x => x.CancelSubscriptionAsync(subscription.StripeSubscriptionId), Times.Once);
        }

        [Fact]
        public async Task CancelSubscription_WithNoUserClaim_ShouldReturnUnauthorized()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Name, "Test User")
            };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);
            _httpContext.User = principal;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };

            // Act
            var result = await _controller.CancelSubscription();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task CancelSubscription_WithNoActiveSubscription_ShouldReturnNotFound()
        {
            // Arrange
            var userId = Guid.Parse(_httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            _mockRepository
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync((PremiumSubscription?)null);

            // Act
            var result = await _controller.CancelSubscription();

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var response = notFoundResult.Value.Should().BeOfType<string>().Subject;
            response.Should().Be("No active subscription found");
        }

        [Fact]
        public async Task CancelSubscription_WithSubscriptionWithoutStripeId_ShouldReturnNotFound()
        {
            // Arrange
            var userId = Guid.Parse(_httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var subscription = new PremiumSubscription
            {
                Id = Guid.NewGuid(),
                StripeSubscriptionId = null
            };

            _mockRepository
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(subscription);

            // Act
            var result = await _controller.CancelSubscription();

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var response = notFoundResult.Value.Should().BeOfType<string>().Subject;
            response.Should().Be("No active subscription found");
        }

        [Fact]
        public async Task CancelSubscription_WithStripeServiceFailure_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = Guid.Parse(_httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var subscription = new PremiumSubscription
            {
                Id = Guid.NewGuid(),
                StripeSubscriptionId = "sub_test123"
            };

            _mockRepository
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(subscription);

            _mockStripeService
                .Setup(x => x.CancelSubscriptionAsync(subscription.StripeSubscriptionId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.CancelSubscription();

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { Error = "Failed to cancel subscription" });
        }

        #endregion

        #region Helper Methods

        private void SetupAuthenticatedUser(Guid userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Name, "Test User")
            };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);
            _httpContext.User = principal;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        #endregion
    }
}
