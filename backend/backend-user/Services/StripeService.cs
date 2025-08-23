using Stripe;
using backend_user.DTO.PremiumSubscription;
using backend_user.Repositories;
using Microsoft.Extensions.Logging;

namespace backend_user.Services
{
    public class StripeService : IStripeService
    {
        private readonly IPremiumSubscriptionRepository _premiumSubscriptionRepository;
        private readonly ILogger<StripeService> _logger;
        private readonly string _stripeSecretKey;
        private readonly string _stripeWebhookSecret;
        private readonly string _priceId;

        public StripeService(
            IPremiumSubscriptionRepository premiumSubscriptionRepository,
            ILogger<StripeService> logger)
        {
            _premiumSubscriptionRepository = premiumSubscriptionRepository ?? throw new ArgumentNullException(nameof(premiumSubscriptionRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY") ?? throw new InvalidOperationException("STRIPE_SECRET_KEY environment variable not configured");
            _stripeWebhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET") ?? throw new InvalidOperationException("STRIPE_WEBHOOK_SECRET environment variable not configured");
            _priceId = Environment.GetEnvironmentVariable("STRIPE_PRICE_ID") ?? throw new InvalidOperationException("STRIPE_PRICE_ID environment variable not configured");
            
            StripeConfiguration.ApiKey = _stripeSecretKey;
        }

        public async Task<string> CreateCheckoutSessionAsync(Guid userId, string successUrl, string cancelUrl)
        {
            _logger.LogInformation("🔍 [DEBUG] CreateCheckoutSessionAsync called for user: {UserId}", userId);
            _logger.LogInformation("🔍 [DEBUG] Success URL: {SuccessUrl}, Cancel URL: {CancelUrl}", successUrl, cancelUrl);
            
            try
            {
                var secretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
                var priceId = Environment.GetEnvironmentVariable("STRIPE_PRICE_ID");
                
                _logger.LogInformation("🔍 [DEBUG] Environment variables - Secret Key exists: {HasSecretKey}, Price ID: {PriceId}", 
                    !string.IsNullOrEmpty(secretKey), priceId);
                
                if (string.IsNullOrEmpty(secretKey))
                {
                    var error = "STRIPE_SECRET_KEY environment variable not configured";
                    _logger.LogError("❌ [DEBUG] {Error}", error);
                    throw new InvalidOperationException(error);
                }

                if (string.IsNullOrEmpty(priceId))
                {
                    var error = "STRIPE_PRICE_ID environment variable not configured";
                    _logger.LogError("❌ [DEBUG] {Error}", error);
                    throw new InvalidOperationException(error);
                }

                StripeConfiguration.ApiKey = secretKey;
                _logger.LogInformation("🔍 [DEBUG] Stripe API key configured successfully");

                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                    {
                        new Stripe.Checkout.SessionLineItemOptions
                        {
                            Price = priceId,
                            Quantity = 1,
                        },
                    },
                    Mode = "subscription",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        { "userId", userId.ToString() }
                    }
                };

                _logger.LogInformation("🔍 [DEBUG] Stripe session options created: {@Options}", options);

                var service = new Stripe.Checkout.SessionService();
                var session = await service.CreateAsync(options);

                _logger.LogInformation("✅ [DEBUG] Stripe checkout session created successfully. Session ID: {SessionId}", session.Id);
                return session.Url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [DEBUG] Error creating Stripe checkout session: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> HandleWebhookAsync(string json, string signature)
        {
            _logger.LogInformation("🔍 [DEBUG] HandleWebhookAsync called with signature: {Signature}", signature);
            
            try
            {
                _logger.LogInformation("🔍 [DEBUG] Constructing Stripe event from JSON and signature");
                var stripeEvent = EventUtility.ConstructEvent(json, signature, _stripeWebhookSecret, throwOnApiVersionMismatch: false);
                
                _logger.LogInformation("🔍 [DEBUG] Stripe event constructed successfully. Event type: {EventType}, Event ID: {EventId}", 
                    stripeEvent.Type, stripeEvent.Id);

                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        _logger.LogInformation("🔍 [DEBUG] Processing CheckoutSessionCompleted event");
                        await HandleCheckoutSessionCompletedAsync(stripeEvent.Data.Object as Stripe.Checkout.Session);
                        break;
                    case "customer.subscription.created":
                        _logger.LogInformation("🔍 [DEBUG] Processing CustomerSubscriptionCreated event");
                        await HandleSubscriptionEventAsync(stripeEvent.Data.Object as Stripe.Subscription);
                        break;
                    case "customer.subscription.updated":
                        _logger.LogInformation("🔍 [DEBUG] Processing CustomerSubscriptionUpdated event");
                        await HandleSubscriptionEventAsync(stripeEvent.Data.Object as Stripe.Subscription);
                        break;
                    case "customer.subscription.deleted":
                        _logger.LogInformation("🔍 [DEBUG] Processing CustomerSubscriptionDeleted event");
                        await HandleSubscriptionCancelledAsync(stripeEvent.Data.Object as Stripe.Subscription);
                        break;
                    default:
                        _logger.LogInformation("🔍 [DEBUG] Unhandled event type: {EventType}", stripeEvent.Type);
                        break;
                }

                _logger.LogInformation("✅ [DEBUG] Webhook processed successfully");
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "❌ [DEBUG] Stripe exception in webhook handling: {Message}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [DEBUG] General exception in webhook handling: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<PremiumSubscriptionDto?> GetSubscriptionAsync(string subscriptionId)
        {
            var service = new SubscriptionService();
            var subscription = await service.GetAsync(subscriptionId);
            
            if (subscription == null)
                return null;

            return new PremiumSubscriptionDto
            {
                StripeSubscriptionId = subscription.Id,
                StripeCustomerId = subscription.CustomerId,
                Status = subscription.Status,
                CurrentPeriodStart = subscription.CurrentPeriodStart,
                CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                CancelAtPeriodEnd = subscription.CancelAtPeriodEnd
            };
        }

        public async Task<bool> CancelSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var service = new SubscriptionService();
                var options = new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = true
                };
                
                await service.UpdateAsync(subscriptionId, options);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task HandleSubscriptionEventAsync(Stripe.Subscription? subscription)
        {
            _logger.LogInformation("🔍 [DEBUG] HandleSubscriptionEventAsync called with subscription: {SubscriptionId}", subscription?.Id);
            
            if (subscription == null)
            {
                _logger.LogWarning("❌ [DEBUG] Subscription is null");
                return;
            }

            // Get userId from metadata instead of ClientReferenceId
            if (!subscription.Metadata.TryGetValue("userId", out var userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                _logger.LogWarning("❌ [DEBUG] No userId in subscription metadata, trying fallback");
                
                // Fallback: try to get from the checkout session
                var checkoutSessionId = subscription.Metadata.GetValueOrDefault("checkout_session_id");
                if (!string.IsNullOrEmpty(checkoutSessionId))
                {
                    _logger.LogInformation("🔍 [DEBUG] Trying to get userId from checkout session: {SessionId}", checkoutSessionId);
                    var sessionService = new Stripe.Checkout.SessionService();
                    var session = await sessionService.GetAsync(checkoutSessionId);
                    if (session.Metadata.TryGetValue("userId", out var sessionUserIdStr) && Guid.TryParse(sessionUserIdStr, out var sessionUserId))
                    {
                        userId = sessionUserId;
                        _logger.LogInformation("🔍 [DEBUG] Got userId from checkout session: {UserId}", userId);
                    }
                    else
                    {
                        _logger.LogWarning("❌ [DEBUG] No userId in checkout session metadata");
                        return; // Cannot determine user ID
                    }
                }
                else
                {
                    _logger.LogWarning("❌ [DEBUG] No checkout_session_id in subscription metadata");
                    return; // Cannot determine user ID
                }
            }
            else
            {
                _logger.LogInformation("🔍 [DEBUG] Got userId from subscription metadata: {UserId}", userId);
            }

            _logger.LogInformation("🔍 [DEBUG] Checking if subscription exists for user: {UserId}", userId);
            var exists = await _premiumSubscriptionRepository.ExistsAsync(userId);
            _logger.LogInformation("🔍 [DEBUG] Subscription exists: {Exists}", exists);

            if (exists)
            {
                _logger.LogInformation("🔍 [DEBUG] Updating existing subscription");
                // Update existing subscription
                var existing = await _premiumSubscriptionRepository.GetByUserIdAsync(userId);
                if (existing != null)
                {
                    _logger.LogInformation("🔍 [DEBUG] Updating subscription ID: {SubscriptionId}", existing.Id);
                    await _premiumSubscriptionRepository.UpdateAsync(existing.Id, new UpdatePremiumSubscriptionDto
                    {
                        StripeSubscriptionId = subscription.Id,
                        StripeCustomerId = subscription.CustomerId,
                        Status = subscription.Status,
                        CurrentPeriodStart = subscription.CurrentPeriodStart,
                        CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                        CancelAtPeriodEnd = subscription.CancelAtPeriodEnd
                    });
                    _logger.LogInformation("✅ [DEBUG] Subscription updated successfully");
                }
            }
            else
            {
                _logger.LogInformation("🔍 [DEBUG] Creating new subscription for user: {UserId}", userId);
                // Create new subscription
                await _premiumSubscriptionRepository.CreateAsync(new CreatePremiumSubscriptionDto
                {
                    UserId = userId,
                    StripeSubscriptionId = subscription.Id,
                    StripeCustomerId = subscription.CustomerId,
                    Status = subscription.Status,
                    CurrentPeriodStart = subscription.CurrentPeriodStart,
                    CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                    CancelAtPeriodEnd = subscription.CancelAtPeriodEnd
                });
                _logger.LogInformation("✅ [DEBUG] New subscription created successfully");
            }
        }

        private async Task HandleSubscriptionCancelledAsync(Stripe.Subscription? subscription)
        {
            if (subscription == null) return;

            var existing = await _premiumSubscriptionRepository.GetByStripeSubscriptionIdAsync(subscription.Id);
            if (existing != null)
            {
                await _premiumSubscriptionRepository.UpdateAsync(existing.Id, new UpdatePremiumSubscriptionDto
                {
                    Status = "cancelled"
                });
            }
        }

        private async Task HandleCheckoutSessionCompletedAsync(Stripe.Checkout.Session? session)
        {
            if (session == null) return;

            _logger.LogInformation("🔍 [DEBUG] Processing CheckoutSessionCompleted event. Session ID: {SessionId}", session.Id);

            // Store the checkout session ID and user ID for later subscription processing
            var userId = Guid.Empty; // Default to empty if not found
            if (session.Metadata.TryGetValue("userId", out var userIdStr) && Guid.TryParse(userIdStr, out var parsedUserId))
            {
                userId = parsedUserId;
                _logger.LogInformation("🔍 [DEBUG] Got userId from checkout session metadata: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("❌ [DEBUG] No userId in checkout session metadata for completed session");
                return; // Cannot determine user ID
            }

            _logger.LogInformation("🔍 [DEBUG] Checking if subscription exists for user: {UserId}", userId);
            var exists = await _premiumSubscriptionRepository.ExistsAsync(userId);
            _logger.LogInformation("🔍 [DEBUG] Subscription exists: {Exists}", exists);

            // Set subscription period to current time (subscription starts now)
            var now = DateTime.UtcNow;
            var subscriptionEnd = now.AddDays(30); // Assuming monthly subscription

            if (exists)
            {
                _logger.LogInformation("🔍 [DEBUG] Updating existing subscription for completed checkout session");
                var existing = await _premiumSubscriptionRepository.GetByUserIdAsync(userId);
                if (existing != null)
                {
                    _logger.LogInformation("🔍 [DEBUG] Updating subscription ID: {SubscriptionId}", existing.Id);
                    await _premiumSubscriptionRepository.UpdateAsync(existing.Id, new UpdatePremiumSubscriptionDto
                    {
                        StripeSubscriptionId = session.Id, // Use session ID as the subscription ID
                        StripeCustomerId = session.CustomerId,
                        Status = "active", // Assuming it's active after completion
                        CurrentPeriodStart = now,
                        CurrentPeriodEnd = subscriptionEnd,
                        CancelAtPeriodEnd = false // No cancellation at this point
                    });
                    _logger.LogInformation("✅ [DEBUG] Subscription updated successfully for completed checkout session");
                }
            }
            else
            {
                _logger.LogInformation("🔍 [DEBUG] Creating new subscription for user: {UserId}", userId);
                await _premiumSubscriptionRepository.CreateAsync(new CreatePremiumSubscriptionDto
                {
                    UserId = userId,
                    StripeSubscriptionId = session.Id,
                    StripeCustomerId = session.CustomerId,
                    Status = "active",
                    CurrentPeriodStart = now,
                    CurrentPeriodEnd = subscriptionEnd,
                    CancelAtPeriodEnd = false
                });
                _logger.LogInformation("✅ [DEBUG] New subscription created successfully for completed checkout session");
            }
        }

        private async Task<string?> GetUserEmailAsync(Guid userId)
        {
            // This would typically come from a user service
            // For now, return null and let Stripe handle it
            return null;
        }
    }
}
