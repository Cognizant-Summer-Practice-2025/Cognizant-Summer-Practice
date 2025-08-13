using backend_user.Services.Abstractions;

namespace backend_user.Services
{
    /// <summary>
    /// Service responsible for determining if request paths require authentication.
    /// </summary>
    public class AuthorizationPathService : IAuthorizationPathService
    {
        private readonly HashSet<string> _publicPaths;
        private readonly HashSet<string> _publicPathPrefixes;

        public AuthorizationPathService()
        {
            // Initialize public paths that don't require authentication
            _publicPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "/"
            };

            // Initialize public path prefixes
            _publicPathPrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "/api/users/login",
                "/api/users/register", 
                "/api/users/oauth-providers/check",
                "/api/users/check-email",
                "/api/oauth/",
                "/api/oauth2/",
                "/openapi",
                "/swagger",
                "/health"
            };
        }

        /// <summary>
        /// Determines if the request requires authentication based on path and method.
        /// </summary>
        /// <param name="context">The HTTP context to evaluate.</param>
        /// <returns>True if authentication is required, false if the path is public.</returns>
        public bool RequiresAuthentication(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            if (string.IsNullOrEmpty(path))
            {
                return true; // Require auth for empty/null paths
            }

            // Check exact public paths
            if (_publicPaths.Contains(path))
            {
                return false;
            }

            // Check public path prefixes
            foreach (var prefix in _publicPathPrefixes)
            {
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            // Special cases for conditional public access
            if (IsConditionallyPublic(path, context.Request.Method))
            {
                return false;
            }

            // Default: require authentication
            return true;
        }

        /// <summary>
        /// Checks for paths that are conditionally public based on method and content.
        /// </summary>
        /// <param name="path">The request path.</param>
        /// <param name="method">The HTTP method.</param>
        /// <returns>True if the path is conditionally public.</returns>
        private static bool IsConditionallyPublic(string path, string method)
        {
            // Allow getUserByEmail during auth
            if (path.Contains("/api/users/email/"))
            {
                return true;
            }

            // Allow OAuth provider lookup during auth (GET only)
            if (path.Contains("/oauth-providers/") && 
                string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Allow public access to user portfolio info for portfolio display (GET only)
            if (path.Contains("/api/users/") && 
                path.Contains("/portfolio-info") && 
                string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Allow public access to user information for portfolio cards (GET only, with restrictions)
            if (path.Contains("/api/users/") && 
                string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase) && 
                !path.Contains("/oauth-providers/") && 
                !path.Contains("/bookmarks/"))
            {
                return true;
            }

            return false;
        }
    }
}
