using System.ComponentModel.DataAnnotations;

namespace backend_user.DTO.OAuth.Request
{
    /// <summary>
    /// Request DTO for refreshing an access token.
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// The refresh token to use for getting a new access token.
        /// </summary>
        [Required]
        public required string RefreshToken { get; set; }
    }
}