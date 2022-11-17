using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Phipseyy.Common.Db.Extensions;
using Phipseyy.Common.Modules;
using Phipseyy.Common.Services;
using Phipseyy.Discord.Services;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using static SpotifyAPI.Web.Scopes;

namespace Phipseyy.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireUserPermission(GuildPermission.Administrator)]
public class AddSpotify : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("add-spotify", "Debug Command - Writes Spotify data to DB")]
    public async Task AddSpotifyCommand(string clientId, string clientSecret)
    {
        var dbContext = DbService.GetDbContext();
        var uri = new Uri($"http://localhost:5000/callback");
        var server = new EmbedIOAuthServer(uri, 5000);

        var (verifier, challenge) = PKCEUtil.GenerateCodes();

        await server.Start();
        server.AuthorizationCodeReceived += async (_, response) =>
        {
            await server.Stop();
            var token = await new OAuthClient().RequestToken(
                new PKCETokenRequest(clientId, response.Code, server.BaseUri, verifier)
            );

            dbContext.SetSpotifyDataToDb(Context.Guild.Id, token, clientId, clientSecret);
            await FollowupAsync(
                text: "Settings saved!",
                ephemeral: true);
            
            PubSubService.AddGuildCommand(Context.Guild);
            Services.PubSubService.StartServiceForGuildCommand(Context.Guild.Id);
        };

        var request = new LoginRequest(server.BaseUri, clientId, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256",
            Scope = new List<string>
            {
                UserReadEmail, UserReadPrivate, PlaylistReadPrivate, PlaylistReadCollaborative,
                UserModifyPlaybackState, UserReadCurrentlyPlaying
            }
        };

        await RespondAsync(
            text: $"To connect your Spotify Account with your Server, login here: {request.ToUri()}",
            ephemeral: true);
    }
}