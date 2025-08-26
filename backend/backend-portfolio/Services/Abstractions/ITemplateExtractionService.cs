using backend_portfolio.DTO.Deployment.Request;
using backend_portfolio.DTO.Deployment.Response;

namespace backend_portfolio.Services.Abstractions
{
    /// <summary>
    /// Service for extracting portfolio template code securely
    /// </summary>
    public interface ITemplateExtractionService
    {
        /// <summary>
        /// Extract template components and code for deployment
        /// </summary>
        /// <param name="request">Template extraction request</param>
        /// <returns>Extracted template data</returns>
        Task<TemplateExtractionResponse> ExtractTemplateAsync(TemplateExtractionRequest request);

        /// <summary>
        /// Validate template exists and is accessible
        /// </summary>
        /// <param name="templateName">Name of the template</param>
        /// <returns>True if template is valid and accessible</returns>
        Task<bool> ValidateTemplateAsync(string templateName);

        /// <summary>
        /// Get list of available templates for deployment
        /// </summary>
        /// <returns>List of available template names</returns>
        Task<List<string>> GetAvailableTemplatesAsync();

        /// <summary>
        /// Create a complete Next.js project structure
        /// </summary>
        /// <param name="templateData">Extracted template data</param>
        /// <param name="portfolioData">User portfolio data</param>
        /// <returns>Complete project structure as file dictionary</returns>
        Task<Dictionary<string, string>> CreateProjectStructureAsync(
            TemplateExtractionResponse templateData, 
            object portfolioData);
    }
}
