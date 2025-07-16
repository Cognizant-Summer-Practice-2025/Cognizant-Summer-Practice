using backend_portfolio.Models;
using backend_portfolio.Repositories;
using backend_portfolio.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IPortfolioTemplateRepository _templateRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IExperienceRepository _experienceRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IBlogPostRepository _blogPostRepository;
        private readonly IBookmarkRepository _bookmarkRepository;

        public PortfolioController(
            IPortfolioRepository portfolioRepository,
            IPortfolioTemplateRepository templateRepository,
            IProjectRepository projectRepository,
            IExperienceRepository experienceRepository,
            ISkillRepository skillRepository,
            IBlogPostRepository blogPostRepository,
            IBookmarkRepository bookmarkRepository)
        {
            _portfolioRepository = portfolioRepository;
            _templateRepository = templateRepository;
            _projectRepository = projectRepository;
            _experienceRepository = experienceRepository;
            _skillRepository = skillRepository;
            _blogPostRepository = blogPostRepository;
            _bookmarkRepository = bookmarkRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPortfolios()
        {
            var portfolios = await _portfolioRepository.GetAllPortfoliosAsync();
            var response = portfolios.Select(p => new PortfolioSummaryDto
            {
                Id = p.Id,
                UserId = p.UserId,
                TemplateId = p.TemplateId,
                Title = p.Title,
                Bio = p.Bio,
                ViewCount = p.ViewCount,
                LikeCount = p.LikeCount,
                Visibility = p.Visibility,
                IsPublished = p.IsPublished,
                UpdatedAt = p.UpdatedAt,
                Template = p.Template != null ? new PortfolioTemplateSummaryDto
                {
                    Id = p.Template.Id,
                    Name = p.Template.Name,
                    Description = p.Template.Description,
                    ComponentName = p.Template.ComponentName,
                    PreviewImageUrl = p.Template.PreviewImageUrl,
                    IsActive = p.Template.IsActive
                } : null
            });
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPortfolioById(Guid id)
        {
            var portfolio = await _portfolioRepository.GetPortfolioByIdAsync(id);
            if (portfolio == null)
            {
                return NotFound($"Portfolio with ID {id} not found.");
            }

            var response = new PortfolioResponseDto
            {
                Id = portfolio.Id,
                UserId = portfolio.UserId,
                TemplateId = portfolio.TemplateId,
                Title = portfolio.Title,
                Bio = portfolio.Bio,
                CustomConfig = portfolio.CustomConfig,
                CustomSections = portfolio.CustomSections,
                ViewCount = portfolio.ViewCount,
                LikeCount = portfolio.LikeCount,
                Visibility = portfolio.Visibility,
                IsPublished = portfolio.IsPublished,
                CreatedAt = portfolio.CreatedAt,
                UpdatedAt = portfolio.UpdatedAt,
                Template = portfolio.Template != null ? new PortfolioTemplateSummaryDto
                {
                    Id = portfolio.Template.Id,
                    Name = portfolio.Template.Name,
                    Description = portfolio.Template.Description,
                    ComponentName = portfolio.Template.ComponentName,
                    PreviewImageUrl = portfolio.Template.PreviewImageUrl,
                    IsActive = portfolio.Template.IsActive
                } : null,
                Projects = portfolio.Projects.Select(p => new ProjectSummaryDto
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
                Experience = portfolio.Experience.Select(e => new ExperienceSummaryDto
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
                Skills = portfolio.Skills.Select(s => new SkillSummaryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Category = s.Category,
                    ProficiencyLevel = s.ProficiencyLevel,
                    DisplayOrder = s.DisplayOrder
                }).ToList(),
                BlogPosts = portfolio.BlogPosts.Select(b => new BlogPostSummaryDto
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

            return Ok(response);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPortfoliosByUserId(Guid userId)
        {
            var portfolios = await _portfolioRepository.GetPortfoliosByUserIdAsync(userId);
            var response = portfolios.Select(p => new PortfolioSummaryDto
            {
                Id = p.Id,
                UserId = p.UserId,
                TemplateId = p.TemplateId,
                Title = p.Title,
                Bio = p.Bio,
                ViewCount = p.ViewCount,
                LikeCount = p.LikeCount,
                Visibility = p.Visibility,
                IsPublished = p.IsPublished,
                UpdatedAt = p.UpdatedAt,
                Template = p.Template != null ? new PortfolioTemplateSummaryDto
                {
                    Id = p.Template.Id,
                    Name = p.Template.Name,
                    Description = p.Template.Description,
                    ComponentName = p.Template.ComponentName,
                    PreviewImageUrl = p.Template.PreviewImageUrl,
                    IsActive = p.Template.IsActive
                } : null
            });
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePortfolio([FromBody] PortfolioRequestDto request)
        {
            try
            {
                var portfolio = await _portfolioRepository.CreatePortfolioAsync(request);
                var response = new PortfolioResponseDto
                {
                    Id = portfolio.Id,
                    UserId = portfolio.UserId,
                    TemplateId = portfolio.TemplateId,
                    Title = portfolio.Title,
                    Bio = portfolio.Bio,
                    CustomConfig = portfolio.CustomConfig,
                    CustomSections = portfolio.CustomSections,
                    ViewCount = portfolio.ViewCount,
                    LikeCount = portfolio.LikeCount,
                    Visibility = portfolio.Visibility,
                    IsPublished = portfolio.IsPublished,
                    CreatedAt = portfolio.CreatedAt,
                    UpdatedAt = portfolio.UpdatedAt
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePortfolio(Guid id, [FromBody] PortfolioUpdateDto request)
        {
            try
            {
                var portfolio = await _portfolioRepository.UpdatePortfolioAsync(id, request);
                if (portfolio == null)
                    return NotFound($"Portfolio with ID {id} not found.");

                var response = new PortfolioResponseDto
                {
                    Id = portfolio.Id,
                    UserId = portfolio.UserId,
                    TemplateId = portfolio.TemplateId,
                    Title = portfolio.Title,
                    Bio = portfolio.Bio,
                    CustomConfig = portfolio.CustomConfig,
                    CustomSections = portfolio.CustomSections,
                    ViewCount = portfolio.ViewCount,
                    LikeCount = portfolio.LikeCount,
                    Visibility = portfolio.Visibility,
                    IsPublished = portfolio.IsPublished,
                    CreatedAt = portfolio.CreatedAt,
                    UpdatedAt = portfolio.UpdatedAt
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePortfolio(Guid id)
        {
            var result = await _portfolioRepository.DeletePortfolioAsync(id);
            if (!result)
                return NotFound($"Portfolio with ID {id} not found.");
            return NoContent();
        }

        [HttpGet("published")]
        public async Task<IActionResult> GetPublishedPortfolios()
        {
            var portfolios = await _portfolioRepository.GetPublishedPortfoliosAsync();
            var response = portfolios.Select(p => new PortfolioSummaryDto
            {
                Id = p.Id,
                UserId = p.UserId,
                TemplateId = p.TemplateId,
                Title = p.Title,
                Bio = p.Bio,
                ViewCount = p.ViewCount,
                LikeCount = p.LikeCount,
                Visibility = p.Visibility,
                IsPublished = p.IsPublished,
                UpdatedAt = p.UpdatedAt,
                Template = p.Template != null ? new PortfolioTemplateSummaryDto
                {
                    Id = p.Template.Id,
                    Name = p.Template.Name,
                    Description = p.Template.Description,
                    ComponentName = p.Template.ComponentName,
                    PreviewImageUrl = p.Template.PreviewImageUrl,
                    IsActive = p.Template.IsActive
                } : null
            });
            return Ok(response);
        }

        [HttpPost("{id}/view")]
        public async Task<IActionResult> IncrementViewCount(Guid id)
        {
            var result = await _portfolioRepository.IncrementViewCountAsync(id);
            if (!result)
                return NotFound($"Portfolio with ID {id} not found.");
            return Ok(new { message = "View count incremented successfully" });
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> IncrementLikeCount(Guid id)
        {
            var result = await _portfolioRepository.IncrementLikeCountAsync(id);
            if (!result)
                return NotFound($"Portfolio with ID {id} not found.");
            return Ok(new { message = "Like count incremented successfully" });
        }

        [HttpPost("{id}/unlike")]
        public async Task<IActionResult> DecrementLikeCount(Guid id)
        {
            var result = await _portfolioRepository.DecrementLikeCountAsync(id);
            if (!result)
                return NotFound($"Portfolio with ID {id} not found.");
            return Ok(new { message = "Like count decremented successfully" });
        }
    }
}
