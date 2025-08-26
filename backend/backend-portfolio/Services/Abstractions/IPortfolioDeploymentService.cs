using backend_portfolio.DTO.Deployment.Request;
using backend_portfolio.DTO.Deployment.Response;

namespace backend_portfolio.Services.Abstractions
{
    /// <summary>
    /// Main service for orchestrating portfolio deployments
    /// </summary>
    public interface IPortfolioDeploymentService
    {
        /// <summary>
        /// Deploy a user's portfolio to Vercel
        /// </summary>
        /// <param name="request">Portfolio deployment request</param>
        /// <returns>Deployment response with URL and status</returns>
        Task<PortfolioDeploymentResponse> DeployUserPortfolioAsync(PortfolioDeploymentRequest request);

        /// <summary>
        /// Get deployment status by deployment ID
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>Current deployment status</returns>
        Task<PortfolioDeploymentResponse?> GetDeploymentStatusAsync(Guid deploymentId);

        /// <summary>
        /// Get all deployments for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user deployments</returns>
        Task<List<DeploymentSummaryResponse>> GetUserDeploymentsAsync(Guid userId);

        /// <summary>
        /// Get all deployments for a portfolio
        /// </summary>
        /// <param name="portfolioId">Portfolio ID</param>
        /// <returns>List of portfolio deployments</returns>
        Task<List<DeploymentSummaryResponse>> GetPortfolioDeploymentsAsync(Guid portfolioId);

        /// <summary>
        /// Cancel a pending or in-progress deployment
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>True if successfully cancelled</returns>
        Task<bool> CancelDeploymentAsync(Guid deploymentId);

        /// <summary>
        /// Delete a deployment and clean up resources
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>True if successfully deleted</returns>
        Task<bool> DeleteDeploymentAsync(Guid deploymentId);

        /// <summary>
        /// Update deployment with custom domain
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <param name="customDomain">Custom domain name</param>
        /// <returns>Updated deployment response</returns>
        Task<PortfolioDeploymentResponse?> UpdateDeploymentDomainAsync(Guid deploymentId, string customDomain);
    }
}
