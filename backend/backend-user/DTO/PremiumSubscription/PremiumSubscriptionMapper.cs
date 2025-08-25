using backend_user.Models;

namespace backend_user.DTO.PremiumSubscription
{
    public static class PremiumSubscriptionMapper
    {
        public static PremiumSubscriptionDto? ToDto(this Models.PremiumSubscription? model)
        {
            if (model == null)
                return null;

            return new PremiumSubscriptionDto
            {
                Id = model.Id,
                UserId = model.UserId,
                StripeSubscriptionId = model.StripeSubscriptionId,
                StripeCustomerId = model.StripeCustomerId,
                Status = model.Status,
                CurrentPeriodStart = model.CurrentPeriodStart,
                CurrentPeriodEnd = model.CurrentPeriodEnd,
                CancelAtPeriodEnd = model.CancelAtPeriodEnd,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt
            };
        }

        public static Models.PremiumSubscription? ToModel(this CreatePremiumSubscriptionDto? dto)
        {
            if (dto == null)
                return null;

            return new Models.PremiumSubscription
            {
                Id = Guid.Empty, // Explicitly set to empty for new models
                UserId = dto.UserId,
                StripeSubscriptionId = dto.StripeSubscriptionId,
                StripeCustomerId = dto.StripeCustomerId,
                Status = dto.Status,
                CurrentPeriodStart = dto.CurrentPeriodStart,
                CurrentPeriodEnd = dto.CurrentPeriodEnd,
                CancelAtPeriodEnd = dto.CancelAtPeriodEnd,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static void UpdateModel(this Models.PremiumSubscription model, UpdatePremiumSubscriptionDto? dto)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            
            if (dto == null)
                return; // Don't update if DTO is null

            // Only update if the property is explicitly set (not null)
            if (dto.StripeSubscriptionId != null)
                model.StripeSubscriptionId = dto.StripeSubscriptionId;
            
            if (dto.StripeCustomerId != null)
                model.StripeCustomerId = dto.StripeCustomerId;
            
            if (!string.IsNullOrEmpty(dto.Status))
                model.Status = dto.Status;
            
            if (dto.CurrentPeriodStart.HasValue)
                model.CurrentPeriodStart = dto.CurrentPeriodStart;
            
            if (dto.CurrentPeriodEnd.HasValue)
                model.CurrentPeriodEnd = dto.CurrentPeriodEnd;
            
            if (dto.CancelAtPeriodEnd.HasValue)
                model.CancelAtPeriodEnd = dto.CancelAtPeriodEnd.Value;
            
            model.UpdatedAt = DateTime.UtcNow;
        }
    }
}
