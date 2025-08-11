using System.ComponentModel.DataAnnotations;

namespace backend_user.DTO.OAuth.Request
{
    /// <summary>
    /// Request DTO for validating an access token.
    /// </summary>
    public class ValidateTokenRequest
    {
        /// <summary>
        /// The access token to validate.
        /// </summary>
        [Required]
        public required string AccessToken { get; set; }
    }
}