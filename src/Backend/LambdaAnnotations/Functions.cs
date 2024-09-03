using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Tomorrowify.Repositories;
using Serilog;
using Microsoft.AspNetCore.Http;
using Tomorrowify.Configuration;
using SpotifyAPI.Web;
using Tomorrowify;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaAnnotations;

/// <summary>
/// A collection of sample Lambda functions that provide a REST api for doing simple math calculations. 
/// </summary>
public class Functions(IRefreshTokenRepository tokenRepository, TomorrowifyConfiguration configuration)
{
    private readonly ILogger _logger = Log.ForContext<Functions>();

    private readonly IRefreshTokenRepository _tokenRepository = tokenRepository;
    private readonly TomorrowifyConfiguration _configuration = configuration;

    [LambdaFunction()]
    [HttpApi(LambdaHttpMethod.Post, "/updatePlaylists")]
    public async Task<IResult> UpdatePlaylistsForAllUsers()
    {
        var tokenDtos = await _tokenRepository.GetAllTokens();

        await Parallel.ForEachAsync(tokenDtos, async (tokenDto, _) =>
        {
            try
            {
                await UpdatePlaylistsForUser(tokenDto.Token, _configuration);
            }
            catch { }
        });

        return Results.Ok();
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
}