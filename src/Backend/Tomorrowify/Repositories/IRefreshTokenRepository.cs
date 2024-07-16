namespace Tomorrowify.Repositories;

public interface IRefreshTokenRepository {
    Task<IEnumerable<string>> GetAllTokens();
    Task<bool> SaveToken(string refreshToken);
}