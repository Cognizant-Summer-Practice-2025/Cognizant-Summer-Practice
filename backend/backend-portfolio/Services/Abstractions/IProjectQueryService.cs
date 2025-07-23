using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.DTO.Experience.Response;
using backend_portfolio.DTO.Skill.Response;
using backend_portfolio.DTO.BlogPost.Response;
using backend_portfolio.DTO.Bookmark.Response;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using backend_portfolio.DTO.ImageUpload.Response;

namespace backend_portfolio.Services.Abstractions
{
    public interface IProjectQueryService
    {
        Task<IEnumerable<ProjectResponse>> GetAllProjectsAsync();
        Task<ProjectResponse?> GetProjectByIdAsync(Guid id);
        Task<IEnumerable<ProjectResponse>> GetProjectsByPortfolioIdAsync(Guid portfolioId);
        Task<IEnumerable<ProjectResponse>> GetFeaturedProjectsAsync();
        Task<IEnumerable<ProjectResponse>> GetFeaturedProjectsByPortfolioIdAsync(Guid portfolioId);
    }
} 