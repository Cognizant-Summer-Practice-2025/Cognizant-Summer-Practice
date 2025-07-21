using System.ComponentModel.DataAnnotations;

namespace backend_user.DTO
{
    public class AddBookmarkRequest
    {
        [Required]
        [StringLength(255)]
        public string PortfolioId { get; set; } = string.Empty;

        [StringLength(255)]
        public string? PortfolioTitle { get; set; }

        [StringLength(255)]
        public string? PortfolioOwnerName { get; set; }
    }

    public class BookmarkResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string PortfolioId { get; set; } = string.Empty;
        public string? PortfolioTitle { get; set; }
        public string? PortfolioOwnerName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class IsBookmarkedResponse
    {
        public bool IsBookmarked { get; set; }
    }
} 