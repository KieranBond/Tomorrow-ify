using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Tomorrowify.Dto;
using Tomorrowify.Repositories.Model;

namespace Tomorrowify.Repositories;

public sealed class DynamoDBRepository : IRefreshTokenRepository
{
    private readonly IAmazonDynamoDB _dynamoDBClient;
    private readonly IDynamoDBContext _dynamoDBContext;
    public DynamoDBRepository()
    {
        var clientConfig = new AmazonDynamoDBConfig
        {
            // Set the endpoint URL to localhost
            ServiceURL = "http://localhost:8000"
        };
        _dynamoDBClient = new AmazonDynamoDBClient(clientConfig);
        _dynamoDBContext = new DynamoDBContext(_dynamoDBClient, new DynamoDBContextConfig
        {
            TableNamePrefix = "Tomorrowify_"
        });
    }

    public async Task<IEnumerable<RefreshTokenDto>> GetAllTokens()
    {
        var result = await _dynamoDBContext.ScanAsync<RefreshToken>(new List<ScanCondition>()).GetRemainingAsync();
        return result.Select(token => new RefreshTokenDto(token.Key, token.Token));
    }

    public async Task SaveToken(string refreshToken)
    {
        var token = new RefreshToken
        {
            Key = Guid.NewGuid().ToString(), // TODO: Use a real key - something user related that won't change
            Token = refreshToken
        };
        
        await _dynamoDBContext.SaveAsync(token);
    }
}