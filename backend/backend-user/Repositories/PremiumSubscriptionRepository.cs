using Microsoft.EntityFrameworkCore;
using backend_user.Data;
using backend_user.Models;
using backend_user.DTO.PremiumSubscription;

namespace backend_user.Repositories
{
    public class PremiumSubscriptionRepository : IPremiumSubscriptionRepository
    {
        private readonly UserDbContext _context;

        public PremiumSubscriptionRepository(UserDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PremiumSubscription?> GetByUserIdAsync(Guid userId)
        {
            return await _context.PremiumSubscriptions
                .FirstOrDefaultAsync(ps => ps.UserId == userId);
        }

        public async Task<PremiumSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
        {
            return await _context.PremiumSubscriptions
                .FirstOrDefaultAsync(ps => ps.StripeSubscriptionId == stripeSubscriptionId);
        }

        public async Task<PremiumSubscription> CreateAsync(CreatePremiumSubscriptionDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
                
            var subscription = dto.ToModel();
            if (subscription == null)
                throw new InvalidOperationException("Failed to create subscription model from DTO");
                
            _context.PremiumSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }

        public async Task<PremiumSubscription> UpdateAsync(Guid id, UpdatePremiumSubscriptionDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
                
            var subscription = await _context.PremiumSubscriptions.FindAsync(id);
            if (subscription == null)
                throw new ArgumentException("Premium subscription not found");

            subscription.UpdateModel(dto);
            await _context.SaveChangesAsync();
            return subscription;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var subscription = await _context.PremiumSubscriptions.FindAsync(id);
            if (subscription == null)
                return false;

            _context.PremiumSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid userId)
        {
            return await _context.PremiumSubscriptions
                .AnyAsync(ps => ps.UserId == userId);
        }

        public async Task<bool> IsActiveAsync(Guid userId)
        {
            var subscription = await _context.PremiumSubscriptions
                .FirstOrDefaultAsync(ps => ps.UserId == userId);

            if (subscription == null)
                return false;

            return subscription.Status == "active" && 
                   (!subscription.CurrentPeriodEnd.HasValue || 
                    subscription.CurrentPeriodEnd > DateTime.UtcNow);
        }
    }
}
