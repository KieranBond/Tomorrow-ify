using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Tomorrowify.Dto;
using Tomorrowify.Repositories.Model;

namespace Tomorrowify.Repositories;

public sealed class DynamoDBRepository : IRefreshTokenRepository
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IDynamoDBContext _dynamoDbContext;

    public DynamoDBRepository(IAmazonDynamoDB dynamoDbClient, IDynamoDBContext dynamoDbContext)
    {
        _dynamoDbClient = dynamoDbClient;
        _dynamoDbContext = dynamoDbContext;
    }

    public async Task<IEnumerable<RefreshTokenDto>> GetAllTokens()
    {
        var result = await _dynamoDbContext.ScanAsync<RefreshToken>(new List<ScanCondition>()).GetRemainingAsync();
        return result.Select(token => new RefreshTokenDto(token.Key, token.Token));
    }

    public async Task SaveToken(string key, string refreshToken)
    {
        var token = new RefreshToken
        {
            Key = key,
            Token = refreshToken
        };

        var existingToken = await _dynamoDbContext.LoadAsync<RefreshToken>(key);
        if(existingToken is not null && existingToken.VersionNumber is not null)
        {
            token = token with { VersionNumber = existingToken.VersionNumber + 1 };
        }
        else
        {
            token.VersionNumber = 1;
        }

        // TODO: Figure out why version checking is causing us issues on updates - we're supposed to be incrementing.
        await _dynamoDbContext.SaveAsync<RefreshToken>(token, new DynamoDBOperationConfig() { SkipVersionCheck = true });
    }
}