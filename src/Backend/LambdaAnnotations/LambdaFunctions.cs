using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Tomorrowify.Repositories;
using Serilog;
using Microsoft.AspNetCore.Http;
using Tomorrowify.Configuration;
using SpotifyAPI.Web;
using Tomorrowify;
using Amazon.Lambda.CloudWatchEvents.ScheduledEvents;
using static SpotifyAPI.Web.PlaylistRemoveItemsRequest;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaAnnotations;

public class LambdaFunctions(IRefreshTokenRepository tokenRepository, TomorrowifyConfiguration configuration)
{
    private ILogger _logger = Log.ForContext<LambdaFunctions>();

    private readonly IRefreshTokenRepository _tokenRepository = tokenRepository;
    private readonly TomorrowifyConfiguration _configuration = configuration;

    [LambdaFunction()]
    public async Task<IResult> UpdatePlaylistsForAllUsers(ScheduledEvent _, ILambdaContext context)
    {
        _logger = _logger.ForContext("LambdaContext", context, true);
        var tokenDtos = await _tokenRepository.GetAllTokens();

        await Parallel.ForEachAsync(tokenDtos, async (tokenDto, _) =>
        {
            try
            {
                await UpdatePlaylistsForUser(tokenDto.Token, _configuration);
                _logger.Information("Completed updating playlists for {user}", tokenDto.Key);
            }
            catch(Exception e)
            {
                _logger.Error(e, "Failure to update playlist for {user}", tokenDto.Key);
            }
        });

        return Results.Ok();
    }

    private async Task<IResult> UpdatePlaylistsForUser(string refreshToken, TomorrowifyConfiguration configuration)
    {
        _logger.Information("Received {refreshToken} for update playlists request", refreshToken);

        // Use the original refresh token to re-auth and get fresh token
        var response = await new OAuthClient()
            .RequestToken(
                new AuthorizationCodeRefreshRequest(
                    Constants.ClientId,
                    configuration.ClientSecret!,
                    refreshToken));

        var spotify = new SpotifyClient(response.AccessToken);
        var user = await spotify.UserProfile.Current();

        _logger.Information("Found {@user} for update playlists request using {refreshToken}", user, refreshToken);

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
                await spotify.Playlists.AddItems(todayPlaylist.Id!,
                    new PlaylistAddItemsRequest(chunk.Select(t => t!.Uri).ToList())
                );

                await spotify.Playlists.RemoveItems(tomorrowPlaylist.Id!,
                    new PlaylistRemoveItemsRequest()
                    {
                        Tracks = chunk.Select(t => new Item() { Uri = t!.Uri }).ToList(),
                    }
                );
            }
        }


        _logger.Information("Completed swap of playlist for {@user}", user);

        return Results.Ok();
    }
}