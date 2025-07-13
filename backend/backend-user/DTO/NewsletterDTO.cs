using System.ComponentModel.DataAnnotations;

namespace backend_user.DTO
{
    // Request DTO
    public class NewsletterCreateRequestDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Type { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class NewsletterUpdateRequestDto
    {
        public bool IsActive { get; set; }
    }

    // Response DTO
    public class NewsletterResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class NewsletterSummaryDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
} 