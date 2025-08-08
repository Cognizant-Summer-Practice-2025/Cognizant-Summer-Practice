using backend_portfolio.DTO;
using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Experience.Request;
using backend_portfolio.DTO.Skill.Request;
using backend_portfolio.DTO.BlogPost.Request;
using backend_portfolio.DTO.Bookmark.Request;
using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.ImageUpload.Request;
using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.DTO.Experience.Response;
using backend_portfolio.DTO.Skill.Response;
using backend_portfolio.DTO.BlogPost.Response;
using backend_portfolio.DTO.Bookmark.Response;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using backend_portfolio.DTO.ImageUpload.Response;
using backend_portfolio.Models;
using backend_portfolio.Services.Abstractions;

namespace backend_portfolio.Services.Mappers
{
    public class ProjectMapper : IProjectMapper
    {
        public ProjectResponse MapToResponseDto(Project entity)
        {
            return new ProjectResponse
            {
                Id = entity.Id,
                PortfolioId = entity.PortfolioId,
                Title = entity.Title,
                Description = entity.Description,
                ImageUrl = entity.ImageUrl,
                DemoUrl = entity.DemoUrl,
                GithubUrl = entity.GithubUrl,
                Technologies = entity.Technologies,
                Featured = entity.Featured,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public IEnumerable<ProjectResponse> MapToResponseDtos(IEnumerable<Project> entities)
        {
            return entities.Select(MapToResponseDto);
        }

        public Project MapFromCreateDto(ProjectCreateRequest createDto)
        {
            return new Project
            {
                Id = Guid.NewGuid(),
                PortfolioId = createDto.PortfolioId,
                Title = createDto.Title,
                Description = createDto.Description,
                ImageUrl = createDto.ImageUrl,
                DemoUrl = createDto.DemoUrl,
                GithubUrl = createDto.GithubUrl,
                Technologies = createDto.Technologies,
                Featured = createDto.Featured,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void UpdateEntityFromDto(Project entity, ProjectUpdateRequest updateDto)
        {
            if (updateDto.Title != null) entity.Title = updateDto.Title;
            if (updateDto.Description != null) entity.Description = updateDto.Description;
            if (updateDto.ImageUrl != null) entity.ImageUrl = updateDto.ImageUrl;
            if (updateDto.DemoUrl != null) entity.DemoUrl = updateDto.DemoUrl;
            if (updateDto.GithubUrl != null) entity.GithubUrl = updateDto.GithubUrl;
            if (updateDto.Technologies != null) entity.Technologies = updateDto.Technologies;
            if (updateDto.Featured.HasValue) entity.Featured = updateDto.Featured.Value;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
} 