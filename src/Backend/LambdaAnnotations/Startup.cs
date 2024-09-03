using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tomorrowify;
using LambdaAnnotations.Configuration;
using Serilog;

namespace LambdaAnnotations;

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        using var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        Log.Logger = logger;

        logger.Information("Starting up Tomorrowify (Lambda annotations)");

        try
        {
            var environment = EnvironmentVariables.ASPNETCORE_ENVIRONMENT;

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddHttpClient();

            logger.Information("Tomorrowify running in {environment}", environment);

            // Guarding only here so that we can get a log out if not set - helps debugging
            Guard.IsNotNullOrWhiteSpace(environment);

            // Fetch configuration from appsettings + environment variables

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables(); // Take precedence over appsettings

            if (environment.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>(); // Client secrets storage in local dev
            }

            var configuration = builder.Build();

            // Set up our required DI
            var tomorrowifyConfig = services.RegisterServices(environment.IsDevelopment(), configuration);

            logger.Information("Tomorrowify defining CORS allowed {origin}", tomorrowifyConfig.WebsiteUri);

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .WithOrigins(tomorrowifyConfig.WebsiteUri)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Unhandled exception");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
