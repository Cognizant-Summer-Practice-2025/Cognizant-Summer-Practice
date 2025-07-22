using backend_portfolio.Models;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Experience.Request;
using backend_portfolio.DTO.Skill.Request;
using backend_portfolio.DTO.BlogPost.Request;

namespace backend_portfolio.DTO.Portfolio.Request
{
    /// <summary>
    /// DTO for creating a new portfolio
    /// </summary>
    public class PortfolioCreateRequest
    {
        public Guid UserId { get; set; }
        public string TemplateName { get; set; } = string.Empty; 
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public Visibility Visibility { get; set; } = Visibility.Public;
        public bool IsPublished { get; set; } = false;
        public string? Components { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing portfolio
    /// </summary>
    public class PortfolioUpdateRequest
    {
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public Visibility? Visibility { get; set; }
        public bool? IsPublished { get; set; }
        public string? Components { get; set; }
        public string? TemplateName { get; set; }
    }

    /// <summary>
    /// DTO for bulk portfolio content creation
    /// </summary>
    public class BulkPortfolioContentRequest
    {
        public Guid PortfolioId { get; set; }
        public List<ProjectCreateRequest>? Projects { get; set; }
        public List<ExperienceCreateRequest>? Experience { get; set; }
        public List<SkillCreateRequest>? Skills { get; set; }
        public List<BlogPostCreateRequest>? BlogPosts { get; set; }
        public bool PublishPortfolio { get; set; } = true;
    }
} 
