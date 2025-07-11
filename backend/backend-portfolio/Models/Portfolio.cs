using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_portfolio.Models
{
    public class Portfolio
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; } // External reference to User Service

        [ForeignKey("Template")]
        public Guid? TemplateId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Bio { get; set; }

        public string? Location { get; set; }

        public string? WebsiteUrl { get; set; }

        public string? Phone { get; set; }

        public Visibility Visibility { get; set; } = Visibility.Public;

        public int ViewCount { get; set; } = 0;

        public int LikeCount { get; set; } = 0;

        public bool IsFeatured { get; set; } = false;

        public bool IsPublished { get; set; } = false;

        public DateTime? PublishedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual PortfolioTemplate? Template { get; set; }
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
        public virtual ICollection<Experience> Experiences { get; set; } = new List<Experience>();
        public virtual ICollection<PortfolioSkill> PortfolioSkills { get; set; } = new List<PortfolioSkill>();
        public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
        public virtual ICollection<PortfolioView> Views { get; set; } = new List<PortfolioView>();
        public virtual ICollection<PortfolioLike> Likes { get; set; } = new List<PortfolioLike>();
        public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public virtual ICollection<SearchQuery> SearchQueries { get; set; } = new List<SearchQuery>();
    }
}
