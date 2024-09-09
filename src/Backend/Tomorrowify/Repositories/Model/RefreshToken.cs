using Amazon.DynamoDBv2.DataModel;

namespace Tomorrowify.Repositories.Model;

[DynamoDBTable(Constants.DynamoDbTableName)]
public sealed record RefreshToken 
{
    [DynamoDBHashKey] //Partition key
    public string Key { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}