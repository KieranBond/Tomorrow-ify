using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Tomorrowify.Repositories;
using Tomorrowify.Repositories.Model;

namespace Tomorrowify.Configuration;

public static class ServiceRegistration
{
    public async static Task<WebApplicationBuilder> RegisterServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(builder.Configuration.Get<TomorrowifyConfiguration>());

        await builder.Services.RegisterDynamoDB();

        return builder;
    }

    private static async Task RegisterDynamoDB(this IServiceCollection serviceCollection)
    {
        var clientConfig = new AmazonDynamoDBConfig()
        {
            ServiceURL = "http://localhost:8000",
        };

        var creds = new BasicAWSCredentials("fakeMyKeyId", "fakeSecretAccessKey");
        var dynamoDbClient = new AmazonDynamoDBClient(creds, clientConfig);
        var dynamoDbContext = new DynamoDBContext(dynamoDbClient, new DynamoDBContextConfig
        {
            //TableNamePrefix = "Tomorrowify_"
        });

        await CreateTokensTable(dynamoDbClient);

        serviceCollection.AddSingleton<IAmazonDynamoDB>(dynamoDbClient);
        serviceCollection.AddSingleton<IDynamoDBContext>(dynamoDbContext);

        serviceCollection.AddScoped<IRefreshTokenRepository, DynamoDBRepository>();
    }

    private static async Task CreateTokensTable(IAmazonDynamoDB dynamoDbClient)
    {
        var tables = await dynamoDbClient.ListTablesAsync();
        if (tables.TableNames.Contains(Constants.DynamoDbTableName))
            return;

        await dynamoDbClient.CreateTableAsync(new CreateTableRequest
        {
            TableName = Constants.DynamoDbTableName,
            KeySchema = new List<KeySchemaElement>
            {
                new() {
                    AttributeName = "Key",
                    KeyType = KeyType.HASH
                }
            },
            AttributeDefinitions = new List<AttributeDefinition>() {
                new () {
                    AttributeName = nameof(RefreshToken.Key),
                    AttributeType = ScalarAttributeType.S
                },
            },
            BillingMode = BillingMode.PAY_PER_REQUEST
        });
    }
}