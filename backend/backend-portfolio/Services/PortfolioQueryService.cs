using backend_portfolio.DTO;
using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.DTO.Experience.Response;
using backend_portfolio.DTO.Skill.Response;
using backend_portfolio.DTO.BlogPost.Response;
using backend_portfolio.DTO.Bookmark.Response;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using backend_portfolio.DTO.ImageUpload.Response;
using backend_portfolio.DTO.Pagination;
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
        private readonly ICacheService _cacheService;
        private readonly IPortfolioMapper _portfolioMapper;
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
            ICacheService cacheService,
            IPortfolioMapper portfolioMapper,
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
            _cacheService = cacheService;
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

        public async Task<PaginatedResponse<PortfolioCardResponse>> GetPortfoliosForHomePagePaginatedAsync(PaginationRequest request)
        {
            try
            {
                // Generate cache key based on request parameters
                var cacheKey = _cacheService.GenerateKey("portfolios_paginated",
                    request.Page,
                    request.PageSize,
                    request.SortBy ?? "most-recent",
                    request.SortDirection ?? "desc",
                    request.SearchTerm ?? "",
                    string.Join(",", request.Skills ?? new List<string>()),
                    string.Join(",", request.Roles ?? new List<string>()),
                    request.Featured?.ToString() ?? "",
                    request.DateFrom?.ToString("yyyy-MM-dd") ?? "",
                    request.DateTo?.ToString("yyyy-MM-dd") ?? "");

                // Try to get from cache first
                var cachedResult = await _cacheService.GetAsync<PaginatedResponse<PortfolioCardResponse>>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation("Returning cached portfolio page {Page} with {Count} items", request.Page, cachedResult.Data.Count);
                    return cachedResult;
                }

                // Get all published portfolios
                var portfolios = await _portfolioRepository.GetPublishedPortfoliosAsync();
                var portfolioCards = new List<PortfolioCardResponse>();

                // Build portfolio cards with all necessary data
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
                        Featured = portfolio.ViewCount > 100 && portfolio.LikeCount > 10, // Simple featured logic based on engagement
                        TemplateName = portfolio.Template?.Name
                    };

                    portfolioCards.Add(portfolioCard);
                }

                // Apply filters
                var filteredCards = ApplyFilters(portfolioCards, request);

                // Apply sorting
                var sortedCards = ApplySorting(filteredCards, request.SortBy, request.SortDirection);

                // Calculate pagination
                var totalCount = sortedCards.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
                var skip = (request.Page - 1) * request.PageSize;
                var paginatedCards = sortedCards.Skip(skip).Take(request.PageSize).ToList();

                var response = new PaginatedResponse<PortfolioCardResponse>
                {
                    Data = paginatedCards,
                    Pagination = new PaginationMetadata
                    {
                        CurrentPage = request.Page,
                        PageSize = request.PageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNext = request.Page < totalPages,
                        HasPrevious = request.Page > 1,
                        SortBy = request.SortBy,
                        SortDirection = request.SortDirection,
                        NextPage = request.Page < totalPages ? request.Page + 1 : null,
                        PreviousPage = request.Page > 1 ? request.Page - 1 : null
                    },
                    CacheKey = cacheKey,
                    CachedAt = DateTime.UtcNow
                };

                // Cache the result for 5 minutes
                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Generated portfolio page {Page} with {Count} items (total: {Total})", 
                    request.Page, paginatedCards.Count, totalCount);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paginated portfolios for home page");
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

        private static IEnumerable<PortfolioCardResponse> ApplyFilters(
            IEnumerable<PortfolioCardResponse> portfolios, 
            PaginationRequest request)
        {
            var filtered = portfolios.AsEnumerable();

            // Search term filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                filtered = filtered.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Role.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm) ||
                    p.Skills.Any(s => s.ToLower().Contains(searchTerm)) ||
                    p.Location.ToLower().Contains(searchTerm));
            }

            // Skills filter
            if (request.Skills != null && request.Skills.Any())
            {
                var skills = request.Skills.Select(s => s.ToLower()).ToList();
                filtered = filtered.Where(p => 
                    p.Skills.Any(skill => skills.Any(filterSkill => 
                        skill.ToLower().Contains(filterSkill))));
            }

            // Roles filter
            if (request.Roles != null && request.Roles.Any())
            {
                var roles = request.Roles.Select(r => r.ToLower()).ToList();
                filtered = filtered.Where(p => 
                    roles.Any(role => p.Role.ToLower().Contains(role)));
            }

            // Featured filter
            if (request.Featured.HasValue)
            {
                filtered = filtered.Where(p => p.Featured == request.Featured.Value);
            }

            // Date range filter
            if (request.DateFrom.HasValue || request.DateTo.HasValue)
            {
                filtered = filtered.Where(p => 
                {
                    if (DateTime.TryParseExact(p.Date, "dd/MM/yyyy", null, 
                        System.Globalization.DateTimeStyles.None, out var portfolioDate))
                    {
                        if (request.DateFrom.HasValue && portfolioDate < request.DateFrom.Value)
                            return false;
                        if (request.DateTo.HasValue && portfolioDate > request.DateTo.Value)
                            return false;
                        return true;
                    }
                    return true; // Include if date can't be parsed
                });
            }

            return filtered;
        }

        private static IEnumerable<PortfolioCardResponse> ApplySorting(
            IEnumerable<PortfolioCardResponse> portfolios, 
            string? sortBy, 
            string? sortDirection)
        {
            var isAscending = sortDirection?.ToLower() == "asc";

            return sortBy?.ToLower() switch
            {
                "most-recent" => isAscending 
                    ? portfolios.OrderBy(p => DateTime.TryParseExact(p.Date, "dd/MM/yyyy", null, 
                        System.Globalization.DateTimeStyles.None, out var date) ? date : DateTime.MinValue)
                    : portfolios.OrderByDescending(p => DateTime.TryParseExact(p.Date, "dd/MM/yyyy", null, 
                        System.Globalization.DateTimeStyles.None, out var date) ? date : DateTime.MinValue),
                        
                "most-popular" => isAscending 
                    ? portfolios.OrderBy(p => p.Views + (p.Likes * 2))
                    : portfolios.OrderByDescending(p => p.Views + (p.Likes * 2)),
                    
                "most-liked" => isAscending 
                    ? portfolios.OrderBy(p => p.Likes).ThenBy(p => p.Views)
                    : portfolios.OrderByDescending(p => p.Likes).ThenByDescending(p => p.Views),
                    
                "most-bookmarked" => isAscending 
                    ? portfolios.OrderBy(p => p.Bookmarks).ThenBy(p => p.Likes).ThenBy(p => p.Views)
                    : portfolios.OrderByDescending(p => p.Bookmarks).ThenByDescending(p => p.Likes).ThenByDescending(p => p.Views),
                    
                "name" => isAscending 
                    ? portfolios.OrderBy(p => p.Name)
                    : portfolios.OrderByDescending(p => p.Name),
                    
                "featured" => isAscending 
                    ? portfolios.OrderBy(p => p.Featured).ThenByDescending(p => DateTime.TryParseExact(p.Date, "dd/MM/yyyy", null, 
                        System.Globalization.DateTimeStyles.None, out var date) ? date : DateTime.MinValue)
                    : portfolios.OrderByDescending(p => p.Featured).ThenByDescending(p => DateTime.TryParseExact(p.Date, "dd/MM/yyyy", null, 
                        System.Globalization.DateTimeStyles.None, out var date) ? date : DateTime.MinValue),
                        
                _ => portfolios.OrderByDescending(p => DateTime.TryParseExact(p.Date, "dd/MM/yyyy", null, 
                    System.Globalization.DateTimeStyles.None, out var date) ? date : DateTime.MinValue)
            };
        }

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