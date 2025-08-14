namespace backend_user.Services.Abstractions
{
    /// <summary>
    /// Interface for configuring security headers on HTTP responses.
    /// Follows Interface Segregation Principle - focused only on security headers.
    /// </summary>
    public interface ISecurityHeadersService
    {
        /// <summary>
        /// Applies security headers to the HTTP response.
        /// </summary>
        /// <param name="context">The HTTP context containing the response to modify.</param>
        void ApplySecurityHeaders(HttpContext context);
    }
}
