using backend_user.Models;

namespace backend_user.DTO.PremiumSubscription
{
    public static class PremiumSubscriptionMapper
    {
        public static PremiumSubscriptionDto ToDto(this Models.PremiumSubscription model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

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

        public static Models.PremiumSubscription ToModel(this CreatePremiumSubscriptionDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return new Models.PremiumSubscription
            {
                UserId = dto.UserId,
                StripeSubscriptionId = dto.StripeSubscriptionId,
                StripeCustomerId = dto.StripeCustomerId,
                Status = dto.Status,
                CurrentPeriodStart = dto.CurrentPeriodStart,
                CurrentPeriodEnd = dto.CurrentPeriodEnd,
                CancelAtPeriodEnd = dto.CancelAtPeriodEnd
            };
        }

        public static void UpdateModel(this Models.PremiumSubscription model, UpdatePremiumSubscriptionDto dto)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (!string.IsNullOrEmpty(dto.StripeSubscriptionId))
                model.StripeSubscriptionId = dto.StripeSubscriptionId;
            
            if (!string.IsNullOrEmpty(dto.StripeCustomerId))
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
