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
        var config = builder.Configuration.Get<TomorrowifyConfiguration>();
        builder.Services.AddSingleton(config);

        await builder.Services.RegisterDynamoDB(builder.Environment.IsDevelopment(), config);

        return builder;
    }

    private static async Task RegisterDynamoDB(this IServiceCollection serviceCollection, bool isDevelopmentEnvironment, TomorrowifyConfiguration configuration)
    {
        var clientConfig = new AmazonDynamoDBConfig();
        AmazonDynamoDBClient dynamoDbClient;

        if (isDevelopmentEnvironment)
        {
            var creds = new BasicAWSCredentials("fakeMyKeyId", "fakeSecretAccessKey");
            clientConfig.ServiceURL = configuration?.Dynamo?.ServiceUrl ?? "http://localhost:8080";
            dynamoDbClient = new AmazonDynamoDBClient(creds, clientConfig);
        }
        else
        {
            clientConfig.RegionEndpoint = Amazon.RegionEndpoint.EUWest2;
            dynamoDbClient = new AmazonDynamoDBClient(clientConfig);
        }

        var dynamoDbContext = new DynamoDBContext(dynamoDbClient, new DynamoDBContextConfig());

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