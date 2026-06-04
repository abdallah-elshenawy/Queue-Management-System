using Microsoft.EntityFrameworkCore;
using QMS.Domain.IRepositories;
using QMS.Domain.Models;

namespace QMS.Infrastructure.Repositories
{
    public class RefreshTokenRepo : BaseRepository<RefreshToken>, IRefreshTokenRepo
    {
        private readonly ApplicationDbContext context;

        public RefreshTokenRepo(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            return await context.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == token);
        }

        public Task RevokeRefreshTokenAsync(RefreshToken refreshToken)
        {
            refreshToken.IsRevoked = true;
            return Task.CompletedTask;
        }

        // FIX: old implementation called LastOrDefaultAsync() with no filter — it revoked a random
        // user's last token. Now revokes every active token for the given user, which is the correct
        // response to a refresh-token reuse attack.
        public async Task RevokeAllUserTokensAsync(int userId)
        {
            var tokens = await context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
                token.IsRevoked = true;
        }
    }
}