using backend_portfolio.Models;
using backend_portfolio.Repositories;
using backend_portfolio.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
                Components = p.Components,
                Template = p.Template != null ? new PortfolioTemplateSummaryDto
                {
                    Id = p.Template.Id,
                    Name = p.Template.Name,
                    Description = p.Template.Description,
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
                ViewCount = portfolio.ViewCount,
                LikeCount = portfolio.LikeCount,
                Visibility = portfolio.Visibility,
                IsPublished = portfolio.IsPublished,
                CreatedAt = portfolio.CreatedAt,
                UpdatedAt = portfolio.UpdatedAt,
                Components = portfolio.Components,
                Template = portfolio.Template != null ? new PortfolioTemplateSummaryDto
                {
                    Id = portfolio.Template.Id,
                    Name = portfolio.Template.Name,
                    Description = portfolio.Template.Description,

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
                    CategoryType = s.CategoryType,
                    Subcategory = s.Subcategory,
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
                Components = p.Components,
                Template = p.Template != null ? new PortfolioTemplateSummaryDto
                {
                    Id = p.Template.Id,
                    Name = p.Template.Name,
                    Description = p.Template.Description,
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
                    ViewCount = portfolio.ViewCount,
                    LikeCount = portfolio.LikeCount,
                    Visibility = portfolio.Visibility,
                    IsPublished = portfolio.IsPublished,
                    CreatedAt = portfolio.CreatedAt,
                    UpdatedAt = portfolio.UpdatedAt,
                    Components = portfolio.Components
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        [HttpPost("create-and-get-id")]
        public async Task<IActionResult> CreatePortfolioAndGetId([FromBody] PortfolioRequestDto request)
        {
            try
            {
                var portfolio = await _portfolioRepository.CreatePortfolioAsync(request);
                return Ok(new { portfolioId = portfolio.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{portfolioId}/save-content")]
        public async Task<IActionResult> SavePortfolioContent(Guid portfolioId, [FromBody] BulkPortfolioContentDto request)
        {
            try
            {
                // Override the portfolioId in the request with the one from the route
                request.PortfolioId = portfolioId;

                int projectsCreated = 0;
                int experienceCreated = 0;
                int skillsCreated = 0;
                int blogPostsCreated = 0;

                // Create projects
                if (request.Projects != null && request.Projects.Any())
                {
                    foreach (var projectDto in request.Projects)
                    {
                        await _projectRepository.CreateProjectAsync(projectDto);
                        projectsCreated++;
                    }
                }

                // Create experience
                if (request.Experience != null && request.Experience.Any())
                {
                    foreach (var experienceDto in request.Experience)
                    {
                        await _experienceRepository.CreateExperienceAsync(experienceDto);
                        experienceCreated++;
                    }
                }

                // Create skills
                if (request.Skills != null && request.Skills.Any())
                {
                    foreach (var skillDto in request.Skills)
                    {
                        await _skillRepository.CreateSkillAsync(skillDto);
                        skillsCreated++;
                    }
                }

                // Create blog posts
                if (request.BlogPosts != null && request.BlogPosts.Any())
                {
                    foreach (var blogPostDto in request.BlogPosts)
                    {
                        await _blogPostRepository.CreateBlogPostAsync(blogPostDto);
                        blogPostsCreated++;
                    }
                }

                // Publish portfolio if requested
                bool portfolioPublished = false;
                if (request.PublishPortfolio)
                {
                    var updateDto = new PortfolioUpdateDto { IsPublished = true };
                    var updatedPortfolio = await _portfolioRepository.UpdatePortfolioAsync(portfolioId, updateDto);
                    portfolioPublished = updatedPortfolio != null;
                }

                var response = new BulkPortfolioResponseDto
                {
                    Message = "Portfolio content saved successfully",
                    ProjectsCreated = projectsCreated,
                    ExperienceCreated = experienceCreated,
                    SkillsCreated = skillsCreated,
                    BlogPostsCreated = blogPostsCreated,
                    PortfolioPublished = portfolioPublished
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
                    ViewCount = portfolio.ViewCount,
                    LikeCount = portfolio.LikeCount,
                    Visibility = portfolio.Visibility,
                    IsPublished = portfolio.IsPublished,
                    CreatedAt = portfolio.CreatedAt,
                    UpdatedAt = portfolio.UpdatedAt,
                    Components = portfolio.Components
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
                Components = p.Components,
                Template = p.Template != null ? new PortfolioTemplateSummaryDto
                {
                    Id = p.Template.Id,
                    Name = p.Template.Name,
                    Description = p.Template.Description,
                    PreviewImageUrl = p.Template.PreviewImageUrl,
                    IsActive = p.Template.IsActive
                } : null
            });
            return Ok(response);
        }

        [HttpGet("home-page-cards")]
        public async Task<IActionResult> GetPortfoliosForHomePage()
        {
            try
            {
                var portfolios = await _portfolioRepository.GetPublishedPortfoliosAsync();
                var portfolioCards = new List<PortfolioCardDto>();

                // Create HttpClient for calling user service
                using var httpClient = new HttpClient();
                const string userServiceBaseUrl = "http://localhost:5200"; // User service URL

                foreach (var portfolio in portfolios)
                {
                    var skills = await _skillRepository.GetSkillsByPortfolioIdAsync(portfolio.Id);
                    var skillNames = skills.Select(s => s.Name).Take(4).ToList(); // Limit to 4 skills for display

                    // Fetch user information from user service
                    string userName = "Unknown User";
                    string userRole = "Portfolio Creator";
                    string userLocation = "Location not specified";
                    string? userAvatar = null;

                    try
                    {
                        var userResponse = await httpClient.GetAsync($"{userServiceBaseUrl}/api/Users/{portfolio.UserId}");
                        if (userResponse.IsSuccessStatusCode)
                        {
                            var userJson = await userResponse.Content.ReadAsStringAsync();
                            var userInfo = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(userJson);
                            
                            // Extract user information from response
                            if (userInfo.TryGetProperty("username", out var usernameElement))
                                userName = usernameElement.GetString() ?? "Unknown User";
                            
                            if (userInfo.TryGetProperty("professionalTitle", out var titleElement))
                                userRole = titleElement.GetString() ?? "Portfolio Creator";
                            
                            if (userInfo.TryGetProperty("location", out var locationElement))
                                userLocation = locationElement.GetString() ?? "Location not specified";
                            
                            if (userInfo.TryGetProperty("avatarUrl", out var avatarElement))
                                userAvatar = avatarElement.GetString();
                        }
                    }
                    catch (Exception userEx)
                    {
                        // Log the error but continue with default values
                        Console.WriteLine($"Error fetching user data for {portfolio.UserId}: {userEx.Message}");
                    }

                    var portfolioCard = new PortfolioCardDto
                    {
                        Id = portfolio.Id,
                        UserId = portfolio.UserId,
                        Name = userName,
                        Role = userRole,
                        Location = userLocation,
                        Description = portfolio.Bio ?? "A talented professional showcasing their work",
                        Skills = skillNames,
                        Views = portfolio.ViewCount,
                        Likes = portfolio.LikeCount,
                        Comments = 0, // You might want to add a comments system later
                        Date = portfolio.UpdatedAt.ToString("dd/MM/yyyy"),
                        Avatar = userAvatar,
                        Featured = false, // You might want to add a featured flag to the portfolio model
                        TemplateName = portfolio.Template?.Name
                    };

                    portfolioCards.Add(portfolioCard);
                }

                return Ok(portfolioCards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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

        [HttpGet("user/{userId}/comprehensive")]
        public async Task<IActionResult> GetUserPortfolioComprehensive(Guid userId)
        {
            try
            {
                // Get user's portfolios
                var portfolios = await _portfolioRepository.GetPortfoliosByUserIdAsync(userId);
                
                if (!portfolios.Any())
                {
                    return Ok(new UserPortfolioComprehensiveDto
                    {
                        UserId = userId,
                        Portfolios = new List<PortfolioSummaryDto>(),
                        Projects = new List<ProjectResponseDto>(),
                        Experience = new List<ExperienceResponseDto>(),
                        Skills = new List<SkillResponseDto>(),
                        BlogPosts = new List<BlogPostResponseDto>(),
                        Bookmarks = new List<BookmarkResponseDto>(),
                        Templates = new List<PortfolioTemplateSummaryDto>()
                    });
                }

                // Get all portfolio IDs for the user
                var portfolioIds = portfolios.Select(p => p.Id).ToList();

                // Get all projects for user's portfolios
                var allProjects = new List<Project>();
                var allExperience = new List<Experience>();
                var allSkills = new List<Skill>();
                var allBlogPosts = new List<BlogPost>();
                var allBookmarks = new List<Bookmark>();

                foreach (var portfolioId in portfolioIds)
                {
                    var projects = await _projectRepository.GetProjectsByPortfolioIdAsync(portfolioId);
                    var experience = await _experienceRepository.GetExperienceByPortfolioIdAsync(portfolioId);
                    var skills = await _skillRepository.GetSkillsByPortfolioIdAsync(portfolioId);
                    var blogPosts = await _blogPostRepository.GetBlogPostsByPortfolioIdAsync(portfolioId);
                    var bookmarks = await _bookmarkRepository.GetBookmarksByPortfolioIdAsync(portfolioId);

                    allProjects.AddRange(projects);
                    allExperience.AddRange(experience);
                    allSkills.AddRange(skills);
                    allBlogPosts.AddRange(blogPosts);
                    allBookmarks.AddRange(bookmarks);
                }

                // Get unique templates used by the user
                var templateIds = portfolios.Select(p => p.TemplateId)
                                          .Distinct()
                                          .ToList();
                
                var templates = new List<PortfolioTemplate>();
                foreach (var templateId in templateIds)
                {
                    var template = await _templateRepository.GetTemplateByIdAsync(templateId);
                    if (template != null)
                        templates.Add(template);
                }

                // Map to DTOs
                var portfolioResponse = portfolios.Select(p => new PortfolioSummaryDto
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
                    Components = p.Components,
                    Template = p.Template != null ? new PortfolioTemplateSummaryDto
                    {
                        Id = p.Template.Id,
                        Name = p.Template.Name,
                        Description = p.Template.Description,
                        PreviewImageUrl = p.Template.PreviewImageUrl,
                        IsActive = p.Template.IsActive
                    } : null
                }).ToList();

                var projectsResponse = allProjects.Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    PortfolioId = p.PortfolioId,
                    Title = p.Title,
                    Description = p.Description,
                    DemoUrl = p.DemoUrl,
                    GithubUrl = p.GithubUrl,
                    ImageUrl = p.ImageUrl,
                    Technologies = p.Technologies,
                    Featured = p.Featured,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                var experienceResponse = allExperience.Select(e => new ExperienceResponseDto
                {
                    Id = e.Id,
                    PortfolioId = e.PortfolioId,
                    JobTitle = e.JobTitle,
                    CompanyName = e.CompanyName,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsCurrent = e.IsCurrent,
                    Description = e.Description,
                    SkillsUsed = e.SkillsUsed,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                }).ToList();

                var skillsResponse = allSkills.Select(s => new SkillResponseDto
                {
                    Id = s.Id,
                    PortfolioId = s.PortfolioId,
                    Name = s.Name,
                    CategoryType = s.CategoryType,
                    Subcategory = s.Subcategory,
                    Category = s.Category,
                    ProficiencyLevel = s.ProficiencyLevel,
                    DisplayOrder = s.DisplayOrder,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                }).ToList();

                var blogPostsResponse = allBlogPosts.Select(b => new BlogPostResponseDto
                {
                    Id = b.Id,
                    PortfolioId = b.PortfolioId,
                    Title = b.Title,
                    Excerpt = b.Excerpt,
                    Content = b.Content,
                    FeaturedImageUrl = b.FeaturedImageUrl,
                    Tags = b.Tags,
                    IsPublished = b.IsPublished,
                    PublishedAt = b.PublishedAt,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                }).ToList();

                var bookmarksResponse = allBookmarks.Select(b => new BookmarkResponseDto
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    PortfolioId = b.PortfolioId,
                    CollectionName = b.CollectionName,
                    Notes = b.Notes,
                    CreatedAt = b.CreatedAt
                }).ToList();

                var templatesResponse = templates.Select(t => new PortfolioTemplateSummaryDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    PreviewImageUrl = t.PreviewImageUrl,
                    IsActive = t.IsActive
                }).ToList();

                var response = new UserPortfolioComprehensiveDto
                {
                    UserId = userId,
                    Portfolios = portfolioResponse,
                    Projects = projectsResponse,
                    Experience = experienceResponse,
                    Skills = skillsResponse,
                    BlogPosts = blogPostsResponse,
                    Bookmarks = bookmarksResponse,
                    Templates = templatesResponse
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
