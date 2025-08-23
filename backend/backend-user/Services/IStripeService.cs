using backend_user.DTO.PremiumSubscription;

namespace backend_user.Services
{
    public interface IStripeService
    {
        Task<string> CreateCheckoutSessionAsync(Guid userId, string successUrl, string cancelUrl);
        Task<bool> HandleWebhookAsync(string json, string signature);
        Task<PremiumSubscriptionDto?> GetSubscriptionAsync(string subscriptionId);
        Task<bool> CancelSubscriptionAsync(string subscriptionId);
    }
}
