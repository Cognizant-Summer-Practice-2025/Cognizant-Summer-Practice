using System.ComponentModel.DataAnnotations;

namespace backend_user.DTO.Newsletter.Request
{
    /// <summary>
    /// Request DTO for creating newsletter subscriptions.
    /// </summary>
    public class NewsletterCreateRequestDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Type { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
