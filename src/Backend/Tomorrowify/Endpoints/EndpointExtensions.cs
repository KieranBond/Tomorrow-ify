using Serilog;
using SpotifyAPI.Web;
using Tomorrowify.Configuration;
using Tomorrowify.Repositories;

namespace Tomorrowify.Endpoints;

public static class EndpointExtensions {
    public static void RegisterEndpoints(this WebApplication app)
    {
        app.MapPost("/signup/{token}", SignupToken);
        app.MapPost("/updatePlaylists", UpdatePlaylistsForAllUsers);
        app.MapPost("/updatePlaylists/{refreshToken}", UpdatePlaylistsForUser);
    }

    private static async Task<IResult> SignupToken(string token, TomorrowifyConfiguration configuration, IRefreshTokenRepository tokenRepository)
    {
        var logger = Log.Logger;
        
        logger.Information("Processing sign up request for {token}, using {callbackUri}", token, configuration.WebsiteUri);

        var response = await new OAuthClient()
            .RequestToken(
                new AuthorizationCodeTokenRequest(
                    Constants.ClientId,
                    configuration.ClientSecret!,
                    token,
                    new Uri(configuration.WebsiteUri)));

        // We can use this token indefinitely to keep our API calls working without re-auth
        var refreshToken = response.RefreshToken;

        logger.Information("Received {refreshToken} for request with {token}", refreshToken, token);


        var spotify = new SpotifyClient(response.AccessToken);
        var user = await spotify.UserProfile.Current();
        
        await tokenRepository.SaveToken(user.Id, refreshToken);

        logger.Information("Saved {refreshToken} for {userId} with {token}", refreshToken, user.Id, token);

        return Results.Ok(refreshToken);
    }

    private static async Task<IResult> UpdatePlaylistsForUser(string refreshToken, TomorrowifyConfiguration configuration)
    {
        var logger = Log.Logger;

        logger.Information("Received {refreshToken} for update playlists request", refreshToken);

        // Use the original refresh token to re-auth and get fresh token
        var response = await new OAuthClient()
            .RequestToken(
                new AuthorizationCodeRefreshRequest(
                    Constants.ClientId,
                    configuration.ClientSecret!,
                    refreshToken));

        var spotify = new SpotifyClient(response.AccessToken);
        var user = await spotify.UserProfile.Current();
        
        logger.Information("Found {userId} for update playlists request using {refreshToken}", user, refreshToken);

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

        if (!tomorrowTracks.Any())
            return Results.Ok();

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

        return Results.Ok();
    }

    private static async Task<IResult> UpdatePlaylistsForAllUsers(IRefreshTokenRepository tokenRepository, TomorrowifyConfiguration configuration)
    {
        var tokenDtos = await tokenRepository.GetAllTokens();

        await Parallel.ForEachAsync(tokenDtos, async (tokenDto, _) =>
        {
            try
            {
                await UpdatePlaylistsForUser(tokenDto.Token, configuration);
            }
            catch { }
        });

        return Results.Ok();
    }
}
