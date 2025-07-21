using backend_portfolio.DTO.Request;
using backend_portfolio.DTO.Response;

namespace backend_portfolio.Services.Abstractions
{
    public interface IProjectCommandService
    {
        Task<ProjectResponse> CreateProjectAsync(ProjectCreateRequest request);
        Task<ProjectResponse?> UpdateProjectAsync(Guid id, ProjectUpdateRequest request);
        Task<bool> DeleteProjectAsync(Guid id);
    }
} 