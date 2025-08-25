using backend_user.Models;
using backend_user.DTO.PremiumSubscription;

namespace backend_user.Repositories
{
    public interface IPremiumSubscriptionRepository
    {
        Task<PremiumSubscription?> GetByUserIdAsync(Guid userId);
        Task<PremiumSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);
        Task<PremiumSubscription> CreateAsync(CreatePremiumSubscriptionDto dto);
        Task<PremiumSubscription> UpdateAsync(Guid id, UpdatePremiumSubscriptionDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid userId);
        Task<bool> IsActiveAsync(Guid userId);
    }
}
