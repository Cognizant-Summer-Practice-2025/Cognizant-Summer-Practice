using backend_portfolio.Models;
using backend_portfolio.DTO;

namespace backend_portfolio.Repositories
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetAllProjectsAsync();
        Task<Project?> GetProjectByIdAsync(Guid id);
        Task<List<Project>> GetProjectsByPortfolioIdAsync(Guid portfolioId);
        Task<Project> CreateProjectAsync(ProjectRequestDto request);
        Task<Project?> UpdateProjectAsync(Guid id, ProjectUpdateDto request);
        Task<bool> DeleteProjectAsync(Guid id);
        Task<List<Project>> GetFeaturedProjectsAsync();
        Task<List<Project>> GetFeaturedProjectsByPortfolioIdAsync(Guid portfolioId);
    }
}
