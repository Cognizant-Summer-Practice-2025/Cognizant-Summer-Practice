using backend_user.Models;
using System.Security.Claims;

namespace backend_user.Services.Abstractions
{
    /// <summary>
    /// Interface for building claims from user information.
    /// Follows Interface Segregation Principle - focused only on claims generation.
    /// </summary>
    public interface IClaimsBuilderService
    {
        /// <summary>
        /// Builds a collection of claims from user information.
        /// </summary>
        /// <param name="user">The user to build claims for.</param>
        /// <returns>A collection of claims representing the user.</returns>
        IEnumerable<Claim> BuildClaims(User user);

        /// <summary>
        /// Builds a ClaimsPrincipal from user information.
        /// </summary>
        /// <param name="user">The user to build the principal for.</param>
        /// <param name="authenticationType">The authentication type to use.</param>
        /// <returns>A ClaimsPrincipal representing the user.</returns>
        ClaimsPrincipal BuildPrincipal(User user, string authenticationType = "OAuth2");
    }
}
