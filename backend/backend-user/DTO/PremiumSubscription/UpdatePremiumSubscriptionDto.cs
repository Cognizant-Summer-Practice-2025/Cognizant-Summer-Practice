namespace backend_user.DTO.PremiumSubscription
{
    public class UpdatePremiumSubscriptionDto
    {
        public string? StripeSubscriptionId { get; set; }
        public string? StripeCustomerId { get; set; }
        public string? Status { get; set; }
        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public bool? CancelAtPeriodEnd { get; set; }
    }
}
