namespace backend_user.DTO.Newsletter.Request
{
    /// <summary>
    /// Request DTO for updating newsletter subscription settings.
    /// </summary>
    public class NewsletterUpdateRequestDto
    {
        public bool IsActive { get; set; }
    }
}
