namespace backend_user.DTO.OAuthProvider.Request
{
    /// <summary>
    /// Request DTO for updating OAuth provider information.
    /// </summary>
    public class OAuthProviderUpdateRequestDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? TokenExpiresAt { get; set; }
    }
}
