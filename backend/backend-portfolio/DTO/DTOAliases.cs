using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.DTO.Experience.Request;
using backend_portfolio.DTO.Experience.Response;
using backend_portfolio.DTO.Skill.Request;
using backend_portfolio.DTO.Skill.Response;
using backend_portfolio.DTO.BlogPost.Request;
using backend_portfolio.DTO.BlogPost.Response;
using backend_portfolio.DTO.Bookmark.Request;
using backend_portfolio.DTO.Bookmark.Response;
using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.PortfolioTemplate.Response;

namespace backend_portfolio.DTO
{
    using PortfolioRequestDto = PortfolioCreateRequest;
    using PortfolioResponseDto = PortfolioResponse;
    using PortfolioDetailDto = PortfolioDetailResponse;
    using PortfolioSummaryDto = PortfolioSummaryResponse;
    using PortfolioUpdateDto = PortfolioUpdateRequest;
    using PortfolioCardDto = PortfolioCardResponse;
    using BulkPortfolioContentDto = BulkPortfolioContentRequest;
    using BulkPortfolioResponseDto = BulkPortfolioContentResponse;
    using UserPortfolioComprehensiveDto = UserPortfolioComprehensiveResponse;

    using ProjectRequestDto = ProjectCreateRequest;
    using ProjectResponseDto = ProjectResponse;
    using ProjectSummaryDto = ProjectSummaryResponse;
    using ProjectUpdateDto = ProjectUpdateRequest;

    using ExperienceRequestDto = ExperienceCreateRequest;
    using ExperienceResponseDto = ExperienceResponse;
    using ExperienceSummaryDto = ExperienceSummaryResponse;
    using ExperienceUpdateDto = ExperienceUpdateRequest;

    using SkillRequestDto = SkillCreateRequest;
    using SkillResponseDto = SkillResponse;
    using SkillSummaryDto = SkillSummaryResponse;
    using SkillUpdateDto = SkillUpdateRequest;

    using BlogPostRequestDto = BlogPostCreateRequest;
    using BlogPostResponseDto = BlogPostResponse;
    using BlogPostSummaryDto = BlogPostSummaryResponse;
    using BlogPostUpdateDto = BlogPostUpdateRequest;

    using BookmarkRequestDto = BookmarkCreateRequest;
    using BookmarkResponseDto = BookmarkResponse;
    using BookmarkSummaryDto = BookmarkSummaryResponse;
    using BookmarkUpdateDto = BookmarkUpdateRequest;

    using PortfolioTemplateRequestDto = PortfolioTemplateCreateRequest;
    using PortfolioTemplateResponseDto = PortfolioTemplateResponse;
    using PortfolioTemplateSummaryDto = PortfolioTemplateSummaryResponse;
    using PortfolioTemplateUpdateDto = PortfolioTemplateUpdateRequest;
} 