namespace backend_portfolio.DTO.Bookmark.Request
{
    /// <summary>
    /// DTO for creating a new bookmark
    /// </summary>
    public class BookmarkCreateRequest
    {
        public Guid UserId { get; set; }
        public Guid PortfolioId { get; set; }
        public string? CollectionName { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing bookmark
    /// </summary>
    public class BookmarkUpdateRequest
    {
        public string? CollectionName { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for toggling bookmark state (add/remove)
    /// </summary>
    public class BookmarkToggleRequest
    {
        public Guid UserId { get; set; }
        public Guid PortfolioId { get; set; }
        public string? CollectionName { get; set; }
        public string? Notes { get; set; }
    }
} 
