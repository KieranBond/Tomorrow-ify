using SpotifyAPI.Web;
using Tomorrowify.Configuration;
using Tomorrowify.Repositories;

namespace Tomorrowify.Endpoints;

public static class EndpointExtensions {
    public static void RegisterEndpoints(this WebApplication app)
    {
        app.MapPost("/signup/{token}", SignupToken);
        app.MapPost("/updatePlaylists/{refreshToken}", UpdatePlaylists);
    }

    private static async Task<IResult> SignupToken(string token, TomorrowifyConfiguration configuration, IRefreshTokenRepository tokenRepository)
    {
        var response = await new OAuthClient()
            .RequestToken(
                new AuthorizationCodeTokenRequest(
                    Constants.ClientId,
                    configuration.ClientSecret!,
                    token,
                    new Uri(configuration.WebsiteUri)));

        // We can use this token indefinitely to keep our API calls working without re-auth
        var refreshToken = response.RefreshToken;

        var spotify = new SpotifyClient(response.AccessToken);
        var user = await spotify.UserProfile.Current();
        
        await tokenRepository.SaveToken(user.Id, refreshToken);

        return Results.Ok(refreshToken);
    }

    private static async Task<IResult> UpdatePlaylists(string refreshToken, TomorrowifyConfiguration configuration)
    {
        // Use the original refresh token to re-auth and get fresh token
        var response = await new OAuthClient()
            .RequestToken(
                new AuthorizationCodeRefreshRequest(
                    Constants.ClientId,
                    configuration.ClientSecret!,
                    refreshToken));

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

        return Results.Ok($"Hello!");
    }
}
