namespace backend_user.DTO.Newsletter.Response
{
    /// <summary>
    /// Summarized newsletter information for list views.
    /// </summary>
    public class NewsletterSummaryDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
