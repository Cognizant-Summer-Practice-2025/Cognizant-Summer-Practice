using backend_portfolio.DTO.Deployment.Request;
using backend_portfolio.DTO.Deployment.Response;

namespace backend_portfolio.Services.Abstractions
{
    /// <summary>
    /// Service for deploying portfolios to Vercel
    /// </summary>
    public interface IVercelDeploymentService
    {
        /// <summary>
        /// Deploy a portfolio to Vercel
        /// </summary>
        /// <param name="request">Deployment request</param>
        /// <returns>Deployment response with URL and status</returns>
        Task<PortfolioDeploymentResponse> DeployPortfolioAsync(PortfolioDeploymentRequest request);

        /// <summary>
        /// Create a new Vercel project
        /// </summary>
        /// <param name="projectName">Name of the project</param>
        /// <param name="files">Project files</param>
        /// <param name="config">Deployment configuration</param>
        /// <returns>Vercel project information</returns>
        Task<VercelDeploymentInfo> CreateVercelProjectAsync(
            string projectName, 
            Dictionary<string, string> files,
            VercelDeploymentConfig config);

        /// <summary>
        /// Get deployment status from Vercel
        /// </summary>
        /// <param name="deploymentId">Vercel deployment ID</param>
        /// <returns>Current deployment status</returns>
        Task<DeploymentStatus> GetDeploymentStatusAsync(string deploymentId);

        /// <summary>
        /// Get deployment logs from Vercel
        /// </summary>
        /// <param name="deploymentId">Vercel deployment ID</param>
        /// <returns>Build and deployment logs</returns>
        Task<List<string>> GetDeploymentLogsAsync(string deploymentId);

        /// <summary>
        /// Delete a Vercel deployment
        /// </summary>
        /// <param name="deploymentId">Vercel deployment ID</param>
        /// <returns>True if successfully deleted</returns>
        Task<bool> DeleteDeploymentAsync(string deploymentId);

        /// <summary>
        /// Set custom domain for deployment
        /// </summary>
        /// <param name="projectId">Vercel project ID</param>
        /// <param name="domain">Custom domain name</param>
        /// <returns>True if domain was set successfully</returns>
        Task<bool> SetCustomDomainAsync(string projectId, string domain);
    }
}
