using Tomorrowify.Repositories;

namespace Tomorrowify.Configuration;

public static class ServiceRegistration
{
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IRefreshTokenRepository, DynamoDBRepository>();
        builder.Services.AddSingleton(builder.Configuration.Get<TomorrowifyConfiguration>());

        return builder;
    }
}