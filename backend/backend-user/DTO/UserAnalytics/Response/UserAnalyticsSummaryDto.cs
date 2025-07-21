namespace backend_user.DTO.UserAnalytics.Response
{
    /// <summary>
    /// Summarized user analytics information for list views.
    /// </summary>
    public class UserAnalyticsSummaryDto
    {
        public Guid Id { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
