namespace backend_user.DTO.Newsletter.Response
{
    /// <summary>
    /// Complete newsletter response DTO with all subscription information.
    /// </summary>
    public class NewsletterResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
