namespace backend_portfolio.DTO.Deployment.Request
{
    /// <summary>
    /// DTO for portfolio deployment request
    /// </summary>
    public class PortfolioDeploymentRequest
    {
        public Guid UserId { get; set; }
        public Guid? PortfolioId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public bool IncludeAllComponents { get; set; } = true;
        public string? ProjectName { get; set; }
        public string? CustomDomain { get; set; }
        public DeploymentEnvironment Environment { get; set; } = DeploymentEnvironment.Production;
    }

    /// <summary>
    /// DTO for template extraction request
    /// </summary>
    public class TemplateExtractionRequest
    {
        public string TemplateName { get; set; } = string.Empty;
        public bool IncludeStyles { get; set; } = true;
        public bool IncludeComponents { get; set; } = true;
        public bool MinifyCode { get; set; } = false;
    }

    /// <summary>
    /// DTO for Vercel deployment configuration
    /// </summary>
    public class VercelDeploymentConfig
    {
        public string ProjectName { get; set; } = string.Empty;
        public string? CustomDomain { get; set; }
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
        public List<string> BuildCommands { get; set; } = new();
        public string OutputDirectory { get; set; } = ".next";
        public string InstallCommand { get; set; } = "npm install";
        public string BuildCommand { get; set; } = "npm run build";
        public string DevCommand { get; set; } = "npm run dev";
        public string Framework { get; set; } = "nextjs";
    }

    public enum DeploymentEnvironment
    {
        Development,
        Staging,
        Production
    }
}
