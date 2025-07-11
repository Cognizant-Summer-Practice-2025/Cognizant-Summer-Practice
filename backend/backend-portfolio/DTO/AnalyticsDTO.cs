namespace backend_portfolio.DTO
{
    // PortfolioView Response DTO
    public class PortfolioViewResponseDto
    {
        public Guid Id { get; set; }
        public Guid PortfolioId { get; set; }
        public Guid? ViewerId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? ReferrerUrl { get; set; }
        public DateTime ViewedAt { get; set; }
    }

    // PortfolioView Request DTO
    public class PortfolioViewRequestDto
    {
        public Guid PortfolioId { get; set; }
        public Guid? ViewerId { get; set; }
        public string? UserAgent { get; set; }
        public string? ReferrerUrl { get; set; }
    }

    // PortfolioLike Response DTO
    public class PortfolioLikeResponseDto
    {
        public Guid Id { get; set; }
        public Guid PortfolioId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // PortfolioLike Request DTO
    public class PortfolioLikeRequestDto
    {
        public Guid PortfolioId { get; set; }
        public Guid UserId { get; set; }
    }

    // Bookmark Response DTO
    public class BookmarkResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PortfolioId { get; set; }
        public string? CollectionName { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public PortfolioSummaryDto? Portfolio { get; set; }
    }

    // Bookmark Request DTO
    public class BookmarkRequestDto
    {
        public Guid UserId { get; set; }
        public Guid PortfolioId { get; set; }
        public string? CollectionName { get; set; }
        public string? Notes { get; set; }
    }

    // Bookmark Summary DTO
    public class BookmarkSummaryDto
    {
        public Guid Id { get; set; }
        public Guid PortfolioId { get; set; }
        public string? CollectionName { get; set; }
        public DateTime CreatedAt { get; set; }
        public PortfolioSummaryDto? Portfolio { get; set; }
    }

    // SearchQuery Response DTO
    public class SearchQueryResponseDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? ClickedPortfolioId { get; set; }
        public string? QueryText { get; set; }
        public string? FiltersApplied { get; set; }
        public int ResultsCount { get; set; }
        public string? SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public PortfolioSummaryDto? ClickedPortfolio { get; set; }
    }

    // SearchQuery Request DTO
    public class SearchQueryRequestDto
    {
        public Guid? UserId { get; set; }
        public Guid? ClickedPortfolioId { get; set; }
        public string? QueryText { get; set; }
        public string? FiltersApplied { get; set; }
        public int ResultsCount { get; set; }
        public string? SessionId { get; set; }
    }

    // SearchQuery Summary DTO
    public class SearchQuerySummaryDto
    {
        public Guid Id { get; set; }
        public string? QueryText { get; set; }
        public int ResultsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 