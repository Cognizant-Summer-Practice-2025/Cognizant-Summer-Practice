using backend_portfolio.Services.Abstractions;

namespace backend_portfolio.Services
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
            _publicPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "/",
                "/openapi",
                "/swagger",
                "/health"
            };

            _publicPathPrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "/openapi",
                "/swagger",
                "/health",
                "/api/portfolio",
                "/api/portfoliotemplate",
                "/api/project",
                "/api/bookmark",
                "/api/image"
            };
        }

        public bool RequiresAuthentication(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            var method = context.Request.Method;

            if (string.IsNullOrEmpty(path))
            {
                return true;
            }

            if (_publicPaths.Contains(path))
            {
                return false;
            }

            foreach (var prefix in _publicPathPrefixes)
            {
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (IsConditionallyPublic(path, method))
                    {
                        return false;
                    }

                    // Default GET for these resources is public unless flagged otherwise
                    if (string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
                    {
                        // Detailed-all is protected
                        if (path.Contains("/detailed-all"))
                        {
                            return true;
                        }
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsConditionallyPublic(string path, string method)
        {
            // Public POSTs
            if (path.StartsWith("/api/portfolio") && string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase) && path.Contains("/view"))
            {
                return true;
            }

            if (path.StartsWith("/api/portfoliotemplate") && string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase) && path.Contains("/seed"))
            {
                return true;
            }

            return false;
        }
    }
}


