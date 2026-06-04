using QMS.Domain.Models;

namespace QMS.Domain.IRepositories
{
    public interface IRefreshTokenRepo : IBaseRepository<RefreshToken>
    {
        Task<RefreshToken> GetByTokenAsync(string token);
        Task RevokeRefreshTokenAsync(RefreshToken refreshToken);

        // FIX: the old RevokeLastRT() grabbed the last row in the whole table regardless of user.
        // On a token-reuse attack we must revoke ALL tokens belonging to that user.
        Task RevokeAllUserTokensAsync(int userId);
    }
}