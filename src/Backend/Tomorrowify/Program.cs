using Tomorrowify;
using Tomorrowify.Configuration;
using Tomorrowify.Endpoints;
using Serilog;

using var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

Log.Logger = logger;

logger.Information("Starting up Tomorrowify");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHttpClient();

    // Makes this work as a Lambda function in AWS or as a normal API on localhost (Kestrel)
    builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

    logger.Information("Tomorrowify running in {environment}", EnvironmentVariables.ASPNETCORE_ENVIRONMENT);

    // Fetch configuration from appsettings + environment variables
    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false)
        .AddJsonFile($"appsettings.{EnvironmentVariables.ASPNETCORE_ENVIRONMENT}.json", optional: true)
        .AddEnvironmentVariables(); // Take precedence over appsettings

    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddUserSecrets<Program>(); // Client secrets storage in local dev
    }

    // Set up our required DI
    var tomorrowifyConfig = await builder.RegisterServices();

    logger.Information("Tomorrowify defining CORS allowed {origin}", tomorrowifyConfig.WebsiteUri);

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder
                .WithOrigins(tomorrowifyConfig.WebsiteUri)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.RegisterEndpoints();

    app.UseCors();

    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Unhandled exception");
}
finally
{
    await Log.CloseAndFlushAsync();
}