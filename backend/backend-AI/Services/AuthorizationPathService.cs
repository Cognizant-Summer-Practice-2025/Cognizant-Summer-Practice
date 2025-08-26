using backend_AI.Services.Abstractions;

namespace backend_AI.Services
{
    /// <summary>
    /// Determines which paths are public vs require authentication for AI backend.
    /// </summary>
    public class AuthorizationPathService : IAuthorizationPathService
    {
        private readonly HashSet<string> _publicPaths;
        private readonly HashSet<string> _publicPrefixes;

        public AuthorizationPathService()
        {
            _publicPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "/",
                "/openapi",
                "/swagger",
                "/health"
            };

            _publicPrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "/openapi",
                "/swagger",
                "/health"
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

            // Allow Airflow to POST tech news using shared secret (handled in controller)
            if (string.Equals(path, "/api/ai/tech-news", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(method, HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (_publicPaths.Contains(path))
            {
                return false;
            }

            foreach (var prefix in _publicPrefixes)
            {
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            // By default, all AI endpoints require authentication
            return true;
        }
    }
}


