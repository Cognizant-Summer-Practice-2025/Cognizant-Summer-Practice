namespace backend_portfolio.DTO.Deployment.Response
{
    /// <summary>
    /// Response DTO for portfolio deployment
    /// </summary>
    public class PortfolioDeploymentResponse
    {
        public Guid DeploymentId { get; set; }
        public Guid PortfolioId { get; set; }
        public Guid UserId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string DeploymentUrl { get; set; } = string.Empty;
        public string? CustomDomain { get; set; }
        public DeploymentStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public VercelDeploymentInfo? VercelInfo { get; set; }
    }

    /// <summary>
    /// Response DTO for template extraction
    /// </summary>
    public class TemplateExtractionResponse
    {
        public string TemplateName { get; set; } = string.Empty;
        public string MainComponent { get; set; } = string.Empty;
        public Dictionary<string, string> Components { get; set; } = new();
        public Dictionary<string, string> Styles { get; set; } = new();
        public List<string> Dependencies { get; set; } = new();
        public string NextConfigJs { get; set; } = string.Empty;
        public string PackageJson { get; set; } = string.Empty;
        public long TotalSizeBytes { get; set; }
        public DateTime ExtractedAt { get; set; }
    }

    /// <summary>
    /// Vercel deployment information
    /// </summary>
    public class VercelDeploymentInfo
    {
        public string VercelProjectId { get; set; } = string.Empty;
        public string VercelDeploymentId { get; set; } = string.Empty;
        public string VercelUrl { get; set; } = string.Empty;
        public string? VercelAlias { get; set; }
        public List<string> BuildLogs { get; set; } = new();
        public TimeSpan BuildDuration { get; set; }
        public string VercelRegion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Deployment status enumeration
    /// </summary>
    public enum DeploymentStatus
    {
        Pending,
        InProgress,
        Building,
        Deploying,
        Completed,
        Failed,
        Cancelled
    }

    /// <summary>
    /// Summary deployment response for list views
    /// </summary>
    public class DeploymentSummaryResponse
    {
        public Guid DeploymentId { get; set; }
        public Guid PortfolioId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string DeploymentUrl { get; set; } = string.Empty;
        public DeploymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
