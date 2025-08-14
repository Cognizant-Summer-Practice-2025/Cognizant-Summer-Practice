namespace backend_user.Services.Abstractions
{
    /// <summary>
    /// Interface for determining if a request path requires authentication.
    /// Follows Interface Segregation Principle - focused only on path authorization logic.
    /// </summary>
    public interface IAuthorizationPathService
    {
        /// <summary>
        /// Determines if the given HTTP context requires authentication.
        /// </summary>
        /// <param name="context">The HTTP context to evaluate.</param>
        /// <returns>True if authentication is required, false if the path is public.</returns>
        bool RequiresAuthentication(HttpContext context);
    }
}
