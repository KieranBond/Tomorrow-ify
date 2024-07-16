using Tomorrowify;
using Tomorrowify.Configuration;
using Tomorrowify.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Makes this work as a Lambda function in AWS or as a normal API on localhost (Kestrel)
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// Fetch configuration from appsettings + environment variables
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{EnvironmentVariables.ASPNETCORE_ENVIRONMENT}.json", optional: true)
    .AddUserSecrets<Program>(optional: true); // Store your client secret in user secrets!

builder.Configuration.AddEnvironmentVariables(); // Take precedence over appsettings

// Set up our required DI
builder.RegisterServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.RegisterEndpoints();

app.Run();