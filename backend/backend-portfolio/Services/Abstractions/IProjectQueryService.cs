using backend_portfolio.DTO.Response;

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