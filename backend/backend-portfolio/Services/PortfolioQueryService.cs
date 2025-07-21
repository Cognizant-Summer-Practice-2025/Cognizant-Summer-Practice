using backend_portfolio.DTO;
using backend_portfolio.DTO.Response;
using backend_portfolio.Repositories;
using backend_portfolio.Services.Abstractions;
using backend_portfolio.Services.Mappers;

namespace backend_portfolio.Services
{
    public class PortfolioQueryService : IPortfolioQueryService
    {
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IExperienceRepository _experienceRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IBlogPostRepository _blogPostRepository;
        private readonly IBookmarkRepository _bookmarkRepository;
        private readonly IPortfolioTemplateRepository _templateRepository;
        private readonly IExternalUserService _externalUserService;
        private readonly PortfolioMapper _portfolioMapper;
        private readonly ILogger<PortfolioQueryService> _logger;

        public PortfolioQueryService(
            IPortfolioRepository portfolioRepository,
            IProjectRepository projectRepository,
            IExperienceRepository experienceRepository,
            ISkillRepository skillRepository,
            IBlogPostRepository blogPostRepository,
            IBookmarkRepository bookmarkRepository,
            IPortfolioTemplateRepository templateRepository,
            IExternalUserService externalUserService,
            PortfolioMapper portfolioMapper,
            ILogger<PortfolioQueryService> logger)
        {
            _portfolioRepository = portfolioRepository;
            _projectRepository = projectRepository;
            _experienceRepository = experienceRepository;
            _skillRepository = skillRepository;
            _blogPostRepository = blogPostRepository;
            _bookmarkRepository = bookmarkRepository;
            _templateRepository = templateRepository;
            _externalUserService = externalUserService;
            _portfolioMapper = portfolioMapper;
            _logger = logger;
        }

        public async Task<IEnumerable<PortfolioSummaryResponse>> GetAllPortfoliosAsync()
        {
            try
            {
                var portfolios = await _portfolioRepository.GetAllPortfoliosAsync();
                return _portfolioMapper.MapToSummaryDtos(portfolios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all portfolios");
                throw;
            }
        }

        public async Task<PortfolioDetailResponse?> GetPortfolioByIdAsync(Guid id)
        {
            try
            {
                var portfolio = await _portfolioRepository.GetPortfolioByIdAsync(id);
                return portfolio != null ? _portfolioMapper.MapToDetailDto(portfolio) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting portfolio by ID: {PortfolioId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<PortfolioSummaryResponse>> GetPortfoliosByUserIdAsync(Guid userId)
        {
            try
            {
                var portfolios = await _portfolioRepository.GetPortfoliosByUserIdAsync(userId);
                return _portfolioMapper.MapToSummaryDtos(portfolios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting portfolios for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<PortfolioSummaryResponse>> GetPublishedPortfoliosAsync()
        {
            try
            {
                var portfolios = await _portfolioRepository.GetPublishedPortfoliosAsync();
                return _portfolioMapper.MapToSummaryDtos(portfolios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting published portfolios");
                throw;
            }
        }

        public async Task<IEnumerable<PortfolioCardResponse>> GetPortfoliosForHomePageAsync()
        {
            try
            {
                var portfolios = await _portfolioRepository.GetPublishedPortfoliosAsync();
                var portfolioCards = new List<PortfolioCardResponse>();

                foreach (var portfolio in portfolios)
                {
                    var skills = await _skillRepository.GetSkillsByPortfolioIdAsync(portfolio.Id);
                    var skillNames = skills.Select(s => s.Name).ToList();
                    var userInfo = await _externalUserService.GetUserInformationAsync(portfolio.UserId);
                    var bookmarks = await _bookmarkRepository.GetBookmarksByPortfolioIdAsync(portfolio.Id);

                    var portfolioCard = new PortfolioCardResponse
                    {
                        Id = portfolio.Id,
                        UserId = portfolio.UserId,
                        Name = userInfo?.FullName ?? "Anonymous User",
                        Role = userInfo?.JobTitle ?? "Professional",
                        Location = userInfo?.Location ?? "Remote",
                        Description = portfolio.Bio ?? "A talented professional showcasing their work",
                        Skills = skillNames,
                        Views = portfolio.ViewCount,
                        Likes = portfolio.LikeCount,
                        Comments = 0, // Future enhancement
                        Bookmarks = bookmarks.Count,
                        Date = portfolio.UpdatedAt.ToString("dd/MM/yyyy"),
                        Avatar = userInfo?.ProfilePictureUrl ?? "/default-avatar.png",
                        Featured = false, // Future enhancement
                        TemplateName = portfolio.Template?.Name
                    };

                    portfolioCards.Add(portfolioCard);
                }

                return portfolioCards;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting portfolios for home page");
                throw;
            }
        }

        public async Task<UserPortfolioComprehensiveResponse> GetUserPortfolioComprehensiveAsync(Guid userId)
        {
            try
            {
                var portfolios = await _portfolioRepository.GetPortfoliosByUserIdAsync(userId);

                if (!portfolios.Any())
                {
                    return CreateEmptyComprehensiveDto(userId);
                }

                var portfolioIds = portfolios.Select(p => p.Id).ToList();
                var allProjects = await GetAllProjectsForPortfolios(portfolioIds);
                var allExperience = await GetAllExperienceForPortfolios(portfolioIds);
                var allSkills = await GetAllSkillsForPortfolios(portfolioIds);
                var allBlogPosts = await GetAllBlogPostsForPortfolios(portfolioIds);
                var allBookmarks = await GetAllBookmarksForPortfolios(portfolioIds);
                var templates = await GetTemplatesForPortfolios(portfolios);

                return new UserPortfolioComprehensiveResponse
                {
                    UserId = userId,
                    Portfolios = _portfolioMapper.MapToSummaryDtos(portfolios).ToList(),
                    Projects = MapProjectsToDto(allProjects),
                    Experience = MapExperienceToDto(allExperience),
                    Skills = MapSkillsToDto(allSkills),
                    BlogPosts = MapBlogPostsToDto(allBlogPosts),
                    Bookmarks = MapBookmarksToDto(allBookmarks),
                    Templates = MapTemplatesToDto(templates)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting comprehensive portfolio data for user: {UserId}", userId);
                throw;
            }
        }

        #region Private Helper Methods

        private static UserPortfolioComprehensiveResponse CreateEmptyComprehensiveDto(Guid userId)
        {
            return new UserPortfolioComprehensiveResponse
            {
                UserId = userId,
                Portfolios = new List<PortfolioSummaryResponse>(),
                Projects = new List<ProjectResponse>(),
                Experience = new List<ExperienceResponse>(),
                Skills = new List<SkillResponse>(),
                BlogPosts = new List<BlogPostResponse>(),
                Bookmarks = new List<BookmarkResponse>(),
                Templates = new List<PortfolioTemplateSummaryResponse>()
            };
        }

        private async Task<List<Models.Project>> GetAllProjectsForPortfolios(List<Guid> portfolioIds)
        {
            var allProjects = new List<Models.Project>();
            foreach (var portfolioId in portfolioIds)
            {
                var projects = await _projectRepository.GetProjectsByPortfolioIdAsync(portfolioId);
                allProjects.AddRange(projects);
            }
            return allProjects;
        }

        private async Task<List<Models.Experience>> GetAllExperienceForPortfolios(List<Guid> portfolioIds)
        {
            var allExperience = new List<Models.Experience>();
            foreach (var portfolioId in portfolioIds)
            {
                var experience = await _experienceRepository.GetExperienceByPortfolioIdAsync(portfolioId);
                allExperience.AddRange(experience);
            }
            return allExperience;
        }

        private async Task<List<Models.Skill>> GetAllSkillsForPortfolios(List<Guid> portfolioIds)
        {
            var allSkills = new List<Models.Skill>();
            foreach (var portfolioId in portfolioIds)
            {
                var skills = await _skillRepository.GetSkillsByPortfolioIdAsync(portfolioId);
                allSkills.AddRange(skills);
            }
            return allSkills;
        }

        private async Task<List<Models.BlogPost>> GetAllBlogPostsForPortfolios(List<Guid> portfolioIds)
        {
            var allBlogPosts = new List<Models.BlogPost>();
            foreach (var portfolioId in portfolioIds)
            {
                var blogPosts = await _blogPostRepository.GetBlogPostsByPortfolioIdAsync(portfolioId);
                allBlogPosts.AddRange(blogPosts);
            }
            return allBlogPosts;
        }

        private async Task<List<Models.Bookmark>> GetAllBookmarksForPortfolios(List<Guid> portfolioIds)
        {
            var allBookmarks = new List<Models.Bookmark>();
            foreach (var portfolioId in portfolioIds)
            {
                var bookmarks = await _bookmarkRepository.GetBookmarksByPortfolioIdAsync(portfolioId);
                allBookmarks.AddRange(bookmarks);
            }
            return allBookmarks;
        }

        private async Task<List<Models.PortfolioTemplate>> GetTemplatesForPortfolios(IEnumerable<Models.Portfolio> portfolios)
        {
            var templateIds = portfolios.Select(p => p.TemplateId).Distinct().ToList();
            var templates = new List<Models.PortfolioTemplate>();

            foreach (var templateId in templateIds)
            {
                var template = await _templateRepository.GetTemplateByIdAsync(templateId);
                if (template != null)
                    templates.Add(template);
            }

            return templates;
        }

        private static List<ProjectResponse> MapProjectsToDto(List<Models.Project> projects)
        {
            return projects.Select(p => new ProjectResponse
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
        }

        private static List<ExperienceResponse> MapExperienceToDto(List<Models.Experience> experiences)
        {
            return experiences.Select(e => new ExperienceResponse
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
        }

        private static List<SkillResponse> MapSkillsToDto(List<Models.Skill> skills)
        {
            return skills.Select(s => new SkillResponse
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
        }

        private static List<BlogPostResponse> MapBlogPostsToDto(List<Models.BlogPost> blogPosts)
        {
            return blogPosts.Select(b => new BlogPostResponse
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
        }

        private static List<BookmarkResponse> MapBookmarksToDto(List<Models.Bookmark> bookmarks)
        {
            return bookmarks.Select(b => new BookmarkResponse
            {
                Id = b.Id,
                UserId = b.UserId,
                PortfolioId = b.PortfolioId,
                CollectionName = b.CollectionName,
                Notes = b.Notes,
                CreatedAt = b.CreatedAt
            }).ToList();
        }

        private static List<PortfolioTemplateSummaryResponse> MapTemplatesToDto(List<Models.PortfolioTemplate> templates)
        {
            return templates.Select(t => new PortfolioTemplateSummaryResponse
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                PreviewImageUrl = t.PreviewImageUrl,
                IsActive = t.IsActive
            }).ToList();
        }

        #endregion
    }
} 