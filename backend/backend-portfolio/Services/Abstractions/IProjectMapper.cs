using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.Models;

namespace backend_portfolio.Services.Abstractions
{
    public interface IProjectMapper : IEntityMapper<Project, ProjectCreateRequest, ProjectResponse, ProjectUpdateRequest>
    {
    }
} 