using Tomorrowify.Dto;

namespace Tomorrowify.Repositories;

public interface IRefreshTokenRepository {
    Task<IEnumerable<RefreshTokenDto>> GetAllTokens();
    Task SaveToken(string key, string refreshToken);
}