using Microsoft.AspNetCore.Http;

namespace backend_AI.Services.Abstractions
{
    /// <summary>
    /// Determines if a given request path requires authentication.
    /// </summary>
    public interface IAuthorizationPathService
    {
        bool RequiresAuthentication(HttpContext context);
    }
}


