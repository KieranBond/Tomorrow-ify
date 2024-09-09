using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tomorrowify;
using Tomorrowify.Configuration;
using Tomorrowify.Repositories;
using Tomorrowify.Repositories.Model;

namespace LambdaAnnotations.Configuration;

public static class ServiceRegistration
{
    public static TomorrowifyConfiguration RegisterServices(this IServiceCollection serviceCollection, bool isDevelopment, IConfigurationRoot configuration)
    {
        var config = configuration.Get<TomorrowifyConfiguration>();
        Guard.IsNotNull(config);
        serviceCollection.AddSingleton(config);

        serviceCollection.RegisterDynamoDB(isDevelopment, config);

        return config;
    }

    private static void RegisterDynamoDB(this IServiceCollection serviceCollection, bool isDevelopmentEnvironment, TomorrowifyConfiguration configuration)
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

        CreateTokensTable(dynamoDbClient);

        serviceCollection.AddSingleton<IAmazonDynamoDB>(dynamoDbClient);
        serviceCollection.AddSingleton<IDynamoDBContext>(dynamoDbContext);

        serviceCollection.AddScoped<IRefreshTokenRepository, DynamoDBRepository>();
    }

    private static void CreateTokensTable(IAmazonDynamoDB dynamoDbClient)
    {
        var tables = dynamoDbClient.ListTablesAsync().Result;
        if (tables.TableNames.Contains(Constants.DynamoDbTableName))
            return;

        _ = dynamoDbClient.CreateTableAsync(new CreateTableRequest
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
        }).Result;
    }
}