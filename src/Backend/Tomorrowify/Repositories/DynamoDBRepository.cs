namespace Tomorrowify.Repositories;

public sealed class DynamoDBRepository : IRefreshTokenRepository
{
    public DynamoDBRepository()
    {

    }

    public Task<IEnumerable<string>> GetAllTokens()
    {
        throw new NotImplementedException();
    }

    public Task<bool> SaveToken(string refreshToken)
    {
        throw new NotImplementedException();
    }
}