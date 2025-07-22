namespace backend_portfolio.DTO.Pagination
{
    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public PaginationMetadata Pagination { get; set; } = new();
        public string? CacheKey { get; set; }
        public DateTime CachedAt { get; set; }
    }

    public class PaginationMetadata
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public int? NextPage { get; set; }
        public int? PreviousPage { get; set; }
    }
}
