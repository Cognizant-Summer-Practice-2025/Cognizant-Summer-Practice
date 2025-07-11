namespace backend_messages.DTO
{
    // UserCache Response DTO
    public class UserCacheResponseDto
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    // UserCache Request DTO (for updating cache)
    public class UserCacheRequestDto
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
    }

    // UserCache DTO (simplified for use in other DTO)
    public class UserCacheDto
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
} 