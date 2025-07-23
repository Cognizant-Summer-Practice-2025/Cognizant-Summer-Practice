using System.ComponentModel.DataAnnotations;

namespace backend_portfolio.DTO.Pagination
{
    public class PaginationRequest
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 20;

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
        public int Page { get; set; } = 1;

        [Range(1, MaxPageSize, ErrorMessage = "Page size must be between 1 and 50")]
        public int PageSize 
        { 
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string? SortBy { get; set; } = "most-recent";
        public string? SortDirection { get; set; } = "desc";
        public string? SearchTerm { get; set; }
        public List<string>? Skills { get; set; }
        public List<string>? Roles { get; set; }
        public bool? Featured { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
