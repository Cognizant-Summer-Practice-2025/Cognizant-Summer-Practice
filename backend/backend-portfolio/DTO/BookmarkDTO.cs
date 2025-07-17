namespace backend_portfolio.DTO
{
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
        public Guid UserId { get; set; }
        public Guid PortfolioId { get; set; }
        public string? CollectionName { get; set; }
        public DateTime CreatedAt { get; set; }
        public PortfolioSummaryDto? Portfolio { get; set; }
    }

    // Bookmark Update DTO
    public class BookmarkUpdateDto
    {
        public string? CollectionName { get; set; }
        public string? Notes { get; set; }
    }
}
