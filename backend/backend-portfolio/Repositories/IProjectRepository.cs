using backend_portfolio.Models;
using backend_portfolio.DTO;
using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Experience.Request;
using backend_portfolio.DTO.Skill.Request;
using backend_portfolio.DTO.BlogPost.Request;
using backend_portfolio.DTO.Bookmark.Request;
using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.ImageUpload.Request;

namespace backend_portfolio.Repositories
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetAllProjectsAsync();
        Task<Project?> GetProjectByIdAsync(Guid id);
        Task<List<Project>> GetProjectsByPortfolioIdAsync(Guid portfolioId);
        Task<Project> CreateProjectAsync(ProjectCreateRequest request);
        Task<Project?> UpdateProjectAsync(Guid id, ProjectUpdateRequest request);
        Task<bool> DeleteProjectAsync(Guid id);
        Task<List<Project>> GetFeaturedProjectsAsync();
        Task<List<Project>> GetFeaturedProjectsByPortfolioIdAsync(Guid portfolioId);
    }
}
