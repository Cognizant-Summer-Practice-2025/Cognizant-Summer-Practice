namespace backend_user.DTO.PremiumSubscription
{
    public class PremiumSubscriptionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? StripeSubscriptionId { get; set; }
        public string? StripeCustomerId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public bool CancelAtPeriodEnd { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
