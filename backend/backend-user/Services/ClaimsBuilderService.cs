using backend_user.Models;
using backend_user.Services.Abstractions;
using System.Security.Claims;

namespace backend_user.Services
{
    /// <summary>
    /// Service responsible for building claims from user information.
    /// </summary>
    public class ClaimsBuilderService : IClaimsBuilderService
    {
        /// <summary>
        /// Builds a collection of claims from user information.
        /// </summary>
        /// <param name="user">The user to build claims for.</param>
        /// <returns>A collection of claims representing the user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
        public IEnumerable<Claim> BuildClaims(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.Username),
                new("IsAdmin", user.IsAdmin.ToString())
            };

            // Add optional claims if available
            if (!string.IsNullOrEmpty(user.FirstName))
            {
                claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            }

            if (!string.IsNullOrEmpty(user.LastName))
            {
                claims.Add(new Claim(ClaimTypes.Surname, user.LastName));
            }

            if (!string.IsNullOrEmpty(user.ProfessionalTitle))
            {
                claims.Add(new Claim("ProfessionalTitle", user.ProfessionalTitle));
            }

            if (!string.IsNullOrEmpty(user.Location))
            {
                claims.Add(new Claim("Location", user.Location));
            }

            claims.Add(new Claim("IsActive", user.IsActive.ToString()));

            if (user.LastLoginAt.HasValue)
            {
                claims.Add(new Claim("LastLogin", user.LastLoginAt.Value.ToString("O")));
            }

            return claims;
        }

        /// <summary>
        /// Builds a ClaimsPrincipal from user information.
        /// </summary>
        /// <param name="user">The user to build the principal for.</param>
        /// <param name="authenticationType">The authentication type to use.</param>
        /// <returns>A ClaimsPrincipal representing the user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
        /// <exception cref="ArgumentException">Thrown when authenticationType is null or empty.</exception>
        public ClaimsPrincipal BuildPrincipal(User user, string authenticationType = "OAuth2")
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrEmpty(authenticationType))
            {
                throw new ArgumentException("Authentication type cannot be null or empty", nameof(authenticationType));
            }

            var claims = BuildClaims(user);
            var identity = new ClaimsIdentity(claims, authenticationType);
            return new ClaimsPrincipal(identity);
        }
    }
}
