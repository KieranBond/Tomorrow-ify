using SpotifyAPI.Web;
using Tomorrowify;
using Tomorrowify.Configuration;

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

builder.Services.AddSingleton(builder.Configuration.Get<Configuration>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/{token}", async (string token, Configuration configuration) =>
{
    var response = await new OAuthClient()
        .RequestToken(
            new AuthorizationCodeTokenRequest(
                Constants.ClientId,
                configuration.ClientSecret!,
                token, 
                new Uri("http://127.0.0.1:8080")));

    var spotify = new SpotifyClient(response.AccessToken);
    var user = await spotify.UserProfile.Current();
    var userPlaylists = await spotify.PaginateAll(await spotify.Playlists.CurrentUsers());
    
    var tomorrowPlaylist = 
        userPlaylists.FirstOrDefault(p => p.Name == "Tomorrow") ?? 
        await spotify.Playlists.Create(user.Id, new PlaylistCreateRequest("Tomorrow"));

    var todayPlaylist = 
        userPlaylists.FirstOrDefault(p => p.Name == "Today") ??
        await spotify.Playlists.Create(user.Id, new PlaylistCreateRequest("Today"));

    var tomorrowTracks =
        (await spotify.PaginateAll(await spotify.Playlists.GetItems(tomorrowPlaylist.Id!)))
        .Select(t => t.Track as FullTrack)
        .Where(t => t?.Id != null);

    if !tomorrowTracks.Any()
        return;

     await spotify.Playlists.ReplaceItems(todayPlaylist.Id!,
         new PlaylistReplaceItemsRequest(tomorrowTracks.Take(100).Select(t => t!.Uri).ToList())
     );

    if (tomorrowTracks.Count() > 100)
    {
        var remainingTracks = tomorrowTracks.Skip(100);
        var tracksToAdd = remainingTracks.Chunk(100);
        foreach (var chunk in tracksToAdd)
        {
            await spotify.Playlists.AddItems(tomorrowPlaylist.Id!,
                new PlaylistAddItemsRequest(chunk.Select(t => t!.Uri).ToList())
            );
        }
    }

    return Results.Ok($"Hello!");
});

app.Run();