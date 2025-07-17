using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_portfolio.Models
{
    [Table("portfolios")]
    public class Portfolio
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("template_id")]
        public Guid TemplateId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("bio", TypeName = "text")]
        public string? Bio { get; set; }

        [Column("view_count")]
        public int ViewCount { get; set; } = 0;

        [Column("like_count")]
        public int LikeCount { get; set; } = 0;

        [Column("visibility")]
        public Visibility Visibility { get; set; } = Visibility.Public;

        [Column("is_published")]
        public bool IsPublished { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TemplateId")]
        public virtual PortfolioTemplate Template { get; set; } = null!;
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
        public virtual ICollection<Experience> Experience { get; set; } = new List<Experience>();
        public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
        public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
        public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    }
}
