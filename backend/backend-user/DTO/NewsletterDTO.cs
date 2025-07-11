namespace backend_user.DTO;

// Newsletter Response DTO
public class NewsletterResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Newsletter Request DTO
public class NewsletterRequestDto
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

// Newsletter Summary DTO
public class NewsletterSummaryDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}