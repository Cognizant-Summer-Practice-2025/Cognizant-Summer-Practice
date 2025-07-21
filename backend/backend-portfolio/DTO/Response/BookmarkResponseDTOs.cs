namespace backend_portfolio.DTO.Response
{
    /// <summary>
    /// Standard bookmark response DTO
    /// </summary>
    public class BookmarkResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PortfolioId { get; set; }
        public string? CollectionName { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public PortfolioSummaryResponse? Portfolio { get; set; }
    }

    /// <summary>
    /// Summary bookmark response for list views
    /// </summary>
    public class BookmarkSummaryResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PortfolioId { get; set; }
        public string? CollectionName { get; set; }
        public DateTime CreatedAt { get; set; }
        public PortfolioSummaryResponse? Portfolio { get; set; }
    }
} 