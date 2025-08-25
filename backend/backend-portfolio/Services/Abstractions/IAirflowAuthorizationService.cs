using Microsoft.AspNetCore.Http;

namespace backend_portfolio.Services.Abstractions
{
    public interface IAirflowAuthorizationService
    {
        bool IsServiceToServiceCall(HttpContext context);
        bool IsAuthorizedExternalCall(HttpContext context);
    }
}


