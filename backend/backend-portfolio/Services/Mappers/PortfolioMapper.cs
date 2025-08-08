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
    public class PortfolioMapper : IPortfolioMapper
    {
        public PortfolioResponse MapToResponseDto(Portfolio entity)
        {
            return new PortfolioResponse
            {
                Id = entity.Id,
                UserId = entity.UserId,
                TemplateId = entity.TemplateId,
                Title = entity.Title,
                Bio = entity.Bio,
                ViewCount = entity.ViewCount,
                LikeCount = entity.LikeCount,
                Visibility = entity.Visibility,
                IsPublished = entity.IsPublished,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Components = entity.Components
            };
        }

        public IEnumerable<PortfolioResponse> MapToResponseDtos(IEnumerable<Portfolio> entities)
        {
            return entities.Select(MapToResponseDto);
        }

        public Portfolio MapFromCreateDto(PortfolioCreateRequest createDto)
        {
            return new Portfolio
            {
                Id = Guid.NewGuid(),
                UserId = createDto.UserId,
                Title = createDto.Title,
                Bio = createDto.Bio,
                Visibility = createDto.Visibility,
                IsPublished = createDto.IsPublished,
                Components = createDto.Components,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void UpdateEntityFromDto(Portfolio entity, PortfolioUpdateRequest updateDto)
        {
            if (updateDto.Title != null) entity.Title = updateDto.Title;
            if (updateDto.Bio != null) entity.Bio = updateDto.Bio;
            if (updateDto.Visibility.HasValue) entity.Visibility = updateDto.Visibility.Value;
            if (updateDto.IsPublished.HasValue) entity.IsPublished = updateDto.IsPublished.Value;
            if (updateDto.Components != null) entity.Components = updateDto.Components;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        public PortfolioSummaryResponse MapToSummaryDto(Portfolio entity)
        {
            return new PortfolioSummaryResponse
            {
                Id = entity.Id,
                UserId = entity.UserId,
                TemplateId = entity.TemplateId,
                Title = entity.Title,
                Bio = entity.Bio,
                ViewCount = entity.ViewCount,
                LikeCount = entity.LikeCount,
                Visibility = entity.Visibility,
                IsPublished = entity.IsPublished,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Components = entity.Components,
                Template = entity.Template != null ? new PortfolioTemplateSummaryResponse
                {
                    Id = entity.Template.Id,
                    Name = entity.Template.Name,
                    Description = entity.Template.Description,
                    PreviewImageUrl = entity.Template.PreviewImageUrl,
                    IsActive = entity.Template.IsActive
                } : null
            };
        }

        public IEnumerable<PortfolioSummaryResponse> MapToSummaryDtos(IEnumerable<Portfolio> entities)
        {
            return entities.Select(MapToSummaryDto);
        }

        public PortfolioDetailResponse MapToDetailDto(Portfolio entity)
        {
            return new PortfolioDetailResponse
            {
                Id = entity.Id,
                UserId = entity.UserId,
                TemplateId = entity.TemplateId,
                Title = entity.Title,
                Bio = entity.Bio,
                ViewCount = entity.ViewCount,
                LikeCount = entity.LikeCount,
                Visibility = entity.Visibility,
                IsPublished = entity.IsPublished,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Components = entity.Components,
                Template = entity.Template != null ? new PortfolioTemplateSummaryResponse
                {
                    Id = entity.Template.Id,
                    Name = entity.Template.Name,
                    Description = entity.Template.Description,
                    PreviewImageUrl = entity.Template.PreviewImageUrl,
                    IsActive = entity.Template.IsActive
                } : null,
                Projects = entity.Projects.Select(p => new ProjectSummaryResponse
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    DemoUrl = p.DemoUrl,
                    GithubUrl = p.GithubUrl,
                    Technologies = p.Technologies,
                    Featured = p.Featured
                }).ToList(),
                Experience = entity.Experience.Select(e => new ExperienceSummaryResponse
                {
                    Id = e.Id,
                    JobTitle = e.JobTitle,
                    CompanyName = e.CompanyName,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsCurrent = e.IsCurrent,
                    Description = e.Description,
                    SkillsUsed = e.SkillsUsed
                }).ToList(),
                Skills = entity.Skills.Select(s => new SkillSummaryResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryType = s.CategoryType,
                    Subcategory = s.Subcategory,
                    Category = s.Category,
                    ProficiencyLevel = s.ProficiencyLevel,
                    DisplayOrder = s.DisplayOrder
                }).ToList(),
                BlogPosts = entity.BlogPosts.Select(b => new BlogPostSummaryResponse
                {
                    Id = b.Id,
                    Title = b.Title,
                    Excerpt = b.Excerpt,
                    FeaturedImageUrl = b.FeaturedImageUrl,
                    Tags = b.Tags,
                    IsPublished = b.IsPublished,
                    PublishedAt = b.PublishedAt,
                    UpdatedAt = b.UpdatedAt
                }).ToList()
            };
        }
    }
} 