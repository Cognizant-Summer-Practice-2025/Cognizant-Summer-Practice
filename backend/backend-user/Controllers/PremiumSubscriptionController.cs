using Microsoft.AspNetCore.Mvc;
using backend_user.Services;
using backend_user.Repositories;
using backend_user.DTO.PremiumSubscription;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace backend_user.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PremiumSubscriptionController : ControllerBase
    {
        private readonly IStripeService _stripeService;
        private readonly IPremiumSubscriptionRepository _premiumSubscriptionRepository;
        private readonly ILogger<PremiumSubscriptionController> _logger;

        public PremiumSubscriptionController(
            IStripeService stripeService,
            IPremiumSubscriptionRepository premiumSubscriptionRepository,
            ILogger<PremiumSubscriptionController> logger)
        {
            _stripeService = stripeService;
            _premiumSubscriptionRepository = premiumSubscriptionRepository;
            _logger = logger;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetSubscriptionStatus()
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var isActive = await _premiumSubscriptionRepository.IsActiveAsync(userId.Value);
            return Ok(new { IsPremium = isActive });
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            _logger.LogInformation("🔍 [DEBUG] CreateCheckoutSession called with request: {@Request}", request);
            
            var userId = GetUserIdFromClaims();
            _logger.LogInformation("🔍 [DEBUG] User ID from claims: {UserId}", userId);
            
            if (userId == null)
            {
                _logger.LogWarning("❌ [DEBUG] No user ID found in claims");
                return Unauthorized();
            }

            try
            {
                _logger.LogInformation("🔍 [DEBUG] Calling StripeService.CreateCheckoutSessionAsync for user: {UserId}", userId.Value);
                
                var checkoutUrl = await _stripeService.CreateCheckoutSessionAsync(
                    userId.Value,
                    request.SuccessUrl,
                    request.CancelUrl);

                _logger.LogInformation("✅ [DEBUG] Successfully created checkout session. URL: {CheckoutUrl}", checkoutUrl);
                return Ok(new { CheckoutUrl = checkoutUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [DEBUG] Error creating checkout session for user {UserId}: {Message}", userId.Value, ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhook()
        {
            _logger.LogInformation("🔍 [DEBUG] Webhook received");
            
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                _logger.LogInformation("🔍 [DEBUG] Webhook JSON length: {Length}", json.Length);
                _logger.LogInformation("🔍 [DEBUG] Webhook JSON preview: {Preview}", json.Substring(0, Math.Min(200, json.Length)));
                
                var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
                _logger.LogInformation("🔍 [DEBUG] Stripe signature: {Signature}", signature);

                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("❌ [DEBUG] Missing Stripe signature header");
                    return BadRequest("Missing Stripe signature");
                }

                _logger.LogInformation("🔍 [DEBUG] Calling StripeService.HandleWebhookAsync");
                var success = await _stripeService.HandleWebhookAsync(json, signature);
                
                if (success)
                {
                    _logger.LogInformation("✅ [DEBUG] Webhook processed successfully");
                    return Ok();
                }
                else
                {
                    _logger.LogWarning("❌ [DEBUG] Webhook processing failed");
                    return BadRequest("Webhook processing failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [DEBUG] Exception in webhook handler: {Message}", ex.Message);
                return BadRequest($"Webhook error: {ex.Message}");
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelSubscription()
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var subscription = await _premiumSubscriptionRepository.GetByUserIdAsync(userId.Value);
            if (subscription?.StripeSubscriptionId == null)
                return NotFound("No active subscription found");

            var success = await _stripeService.CancelSubscriptionAsync(subscription.StripeSubscriptionId);
            
            if (success)
                return Ok(new { Message = "Subscription cancelled successfully" });
            else
                return BadRequest(new { Error = "Failed to cancel subscription" });
        }

        private Guid? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;
            return null;
        }
    }

    public class CreateCheckoutSessionRequest
    {
        public string SuccessUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
    }
}
