using backend_portfolio.Services.Abstractions;
using backend_portfolio.DTO.Deployment.Request;
using backend_portfolio.DTO.Deployment.Response;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace backend_portfolio.Controllers
{
    /// <summary>
    /// Controller for portfolio deployment operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DeploymentController : ControllerBase
    {
        private readonly IPortfolioDeploymentService _deploymentService;
        private readonly ITemplateExtractionService _templateExtractionService;
        private readonly ILogger<DeploymentController> _logger;

        public DeploymentController(
            IPortfolioDeploymentService deploymentService,
            ITemplateExtractionService templateExtractionService,
            ILogger<DeploymentController> logger)
        {
            _deploymentService = deploymentService;
            _templateExtractionService = templateExtractionService;
            _logger = logger;
        }

        /// <summary>
        /// Deploy a user's portfolio to Vercel
        /// </summary>
        /// <param name="request">Portfolio deployment request</param>
        /// <returns>Deployment response with URL and status</returns>
        [HttpPost("deploy")]
        public async Task<IActionResult> DeployPortfolio([FromBody] PortfolioDeploymentRequest request)
        {
            try
            {
                _logger.LogInformation("Received deployment request for user {UserId} with template {TemplateName}", 
                    request.UserId, request.TemplateName);

                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (request.UserId == Guid.Empty)
                {
                    return BadRequest(new { error = "User ID is required" });
                }

                if (string.IsNullOrWhiteSpace(request.TemplateName))
                {
                    return BadRequest(new { error = "Template name is required" });
                }

                // Deploy portfolio
                var result = await _deploymentService.DeployUserPortfolioAsync(request);

                if (result.Status == DeploymentStatus.Failed)
                {
                    return StatusCode(500, new 
                    { 
                        error = "Deployment failed", 
                        message = result.ErrorMessage,
                        deploymentId = result.DeploymentId
                    });
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid deployment request");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deploying portfolio");
                return StatusCode(500, new { error = "Internal server error occurred during deployment" });
            }
        }

        /// <summary>
        /// Get deployment status by deployment ID
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>Current deployment status</returns>
        [HttpGet("{deploymentId}/status")]
        public async Task<IActionResult> GetDeploymentStatus([FromRoute] Guid deploymentId)
        {
            try
            {
                if (deploymentId == Guid.Empty)
                {
                    return BadRequest(new { error = "Valid deployment ID is required" });
                }

                var deployment = await _deploymentService.GetDeploymentStatusAsync(deploymentId);

                if (deployment == null)
                {
                    return NotFound(new { error = "Deployment not found" });
                }

                return Ok(deployment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deployment status for {DeploymentId}", deploymentId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all deployments for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user deployments</returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserDeployments([FromRoute] Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    return BadRequest(new { error = "Valid user ID is required" });
                }

                var deployments = await _deploymentService.GetUserDeploymentsAsync(userId);
                return Ok(deployments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deployments for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all deployments for a portfolio
        /// </summary>
        /// <param name="portfolioId">Portfolio ID</param>
        /// <returns>List of portfolio deployments</returns>
        [HttpGet("portfolio/{portfolioId}")]
        public async Task<IActionResult> GetPortfolioDeployments([FromRoute] Guid portfolioId)
        {
            try
            {
                if (portfolioId == Guid.Empty)
                {
                    return BadRequest(new { error = "Valid portfolio ID is required" });
                }

                var deployments = await _deploymentService.GetPortfolioDeploymentsAsync(portfolioId);
                return Ok(deployments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deployments for portfolio {PortfolioId}", portfolioId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Cancel a pending or in-progress deployment
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>Success status</returns>
        [HttpPost("{deploymentId}/cancel")]
        public async Task<IActionResult> CancelDeployment([FromRoute] Guid deploymentId)
        {
            try
            {
                if (deploymentId == Guid.Empty)
                {
                    return BadRequest(new { error = "Valid deployment ID is required" });
                }

                var success = await _deploymentService.CancelDeploymentAsync(deploymentId);

                if (!success)
                {
                    return BadRequest(new { error = "Unable to cancel deployment. It may have already completed or failed." });
                }

                return Ok(new { message = "Deployment cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling deployment {DeploymentId}", deploymentId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a deployment and clean up resources
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{deploymentId}")]
        public async Task<IActionResult> DeleteDeployment([FromRoute] Guid deploymentId)
        {
            try
            {
                if (deploymentId == Guid.Empty)
                {
                    return BadRequest(new { error = "Valid deployment ID is required" });
                }

                var success = await _deploymentService.DeleteDeploymentAsync(deploymentId);

                if (!success)
                {
                    return NotFound(new { error = "Deployment not found" });
                }

                return Ok(new { message = "Deployment deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting deployment {DeploymentId}", deploymentId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Update deployment with custom domain
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <param name="request">Domain update request</param>
        /// <returns>Updated deployment response</returns>
        [HttpPut("{deploymentId}/domain")]
        public async Task<IActionResult> UpdateDeploymentDomain(
            [FromRoute] Guid deploymentId, 
            [FromBody] UpdateDomainRequest request)
        {
            try
            {
                if (deploymentId == Guid.Empty)
                {
                    return BadRequest(new { error = "Valid deployment ID is required" });
                }

                if (string.IsNullOrWhiteSpace(request.CustomDomain))
                {
                    return BadRequest(new { error = "Custom domain is required" });
                }

                var updatedDeployment = await _deploymentService.UpdateDeploymentDomainAsync(
                    deploymentId, 
                    request.CustomDomain);

                if (updatedDeployment == null)
                {
                    return NotFound(new { error = "Deployment not found or domain update failed" });
                }

                return Ok(updatedDeployment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating domain for deployment {DeploymentId}", deploymentId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get list of available templates for deployment
        /// </summary>
        /// <returns>List of available template names</returns>
        [HttpGet("templates")]
        public async Task<IActionResult> GetAvailableTemplates()
        {
            try
            {
                var templates = await _templateExtractionService.GetAvailableTemplatesAsync();
                return Ok(new { templates });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available templates");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Extract template for preview (without deployment)
        /// </summary>
        /// <param name="request">Template extraction request</param>
        /// <returns>Extracted template data</returns>
        [HttpPost("templates/extract")]
        public async Task<IActionResult> ExtractTemplate([FromBody] TemplateExtractionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(request.TemplateName))
                {
                    return BadRequest(new { error = "Template name is required" });
                }

                var templateData = await _templateExtractionService.ExtractTemplateAsync(request);
                return Ok(templateData);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid template extraction request");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting template");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Validate if a template exists and is accessible
        /// </summary>
        /// <param name="templateName">Template name to validate</param>
        /// <returns>Validation result</returns>
        [HttpGet("templates/{templateName}/validate")]
        public async Task<IActionResult> ValidateTemplate([FromRoute] string templateName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(templateName))
                {
                    return BadRequest(new { error = "Template name is required" });
                }

                var isValid = await _templateExtractionService.ValidateTemplateAsync(templateName);
                return Ok(new { isValid, templateName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating template {TemplateName}", templateName);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    /// <summary>
    /// Request DTO for updating deployment domain
    /// </summary>
    public class UpdateDomainRequest
    {
        [Required]
        [StringLength(253, MinimumLength = 4)]
        public string CustomDomain { get; set; } = string.Empty;
    }
}
