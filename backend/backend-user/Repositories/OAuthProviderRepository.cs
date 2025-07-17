using Microsoft.EntityFrameworkCore;
using backend_user.Data;
using backend_user.Models;
using backend_user.DTO;

namespace backend_user.Repositories
{
    public class OAuthProviderRepository : IOAuthProviderRepository
    {
        private readonly UserDbContext _context;

        public OAuthProviderRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<OAuthProvider?> GetByIdAsync(Guid id)
        {
            return await _context.OAuthProviders
                .Include(op => op.User)
                .FirstOrDefaultAsync(op => op.Id == id);
        }

        public async Task<IEnumerable<OAuthProvider>> GetByUserIdAsync(Guid userId)
        {
            return await _context.OAuthProviders
                .Where(op => op.UserId == userId)
                .OrderBy(op => op.CreatedAt)
                .ToListAsync();
        }

        public async Task<OAuthProvider?> GetByUserIdAndProviderAsync(Guid userId, OAuthProviderType provider)
        {
            return await _context.OAuthProviders
                .FirstOrDefaultAsync(op => op.UserId == userId && op.Provider == provider);
        }

        public async Task<OAuthProvider?> GetByProviderAndProviderIdAsync(OAuthProviderType provider, string providerId)
        {
            return await _context.OAuthProviders
                .Include(op => op.User)
                .FirstOrDefaultAsync(op => op.Provider == provider && op.ProviderId == providerId);
        }

        public async Task<OAuthProvider?> GetByProviderAndEmailAsync(OAuthProviderType provider, string email)
        {
            return await _context.OAuthProviders
                .Include(op => op.User)
                .FirstOrDefaultAsync(op => op.Provider == provider && op.ProviderEmail == email);
        }

        public async Task<OAuthProvider> CreateAsync(OAuthProviderCreateRequestDto request)
        {
            var oauthProvider = new OAuthProvider
            {
                UserId = request.UserId,
                Provider = request.Provider,
                ProviderId = request.ProviderId,
                ProviderEmail = request.ProviderEmail,
                AccessToken = request.AccessToken,
                RefreshToken = request.RefreshToken,
                TokenExpiresAt = request.TokenExpiresAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.OAuthProviders.Add(oauthProvider);
            await _context.SaveChangesAsync();
            return oauthProvider;
        }

        public async Task<OAuthProvider?> UpdateAsync(Guid id, OAuthProviderUpdateRequestDto request)
        {
            var oauthProvider = await _context.OAuthProviders.FindAsync(id);
            if (oauthProvider == null)
                return null;

            if (!string.IsNullOrEmpty(request.AccessToken))
                oauthProvider.AccessToken = request.AccessToken;
            
            if (request.RefreshToken != null)
                oauthProvider.RefreshToken = request.RefreshToken;
            
            if (request.TokenExpiresAt.HasValue)
                oauthProvider.TokenExpiresAt = request.TokenExpiresAt;

            oauthProvider.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return oauthProvider;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var oauthProvider = await _context.OAuthProviders.FindAsync(id);
            if (oauthProvider == null)
                return false;

            _context.OAuthProviders.Remove(oauthProvider);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(OAuthProviderType provider, string providerId)
        {
            return await _context.OAuthProviders
                .AnyAsync(op => op.Provider == provider && op.ProviderId == providerId);
        }
    }
} 