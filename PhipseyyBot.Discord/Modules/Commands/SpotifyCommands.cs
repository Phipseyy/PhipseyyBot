#nullable disable
using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using PhipseyyBot.Common;
using PhipseyyBot.Common.Db.Extensions;
using PhipseyyBot.Common.Services;
using PhipseyyBot.Discord.Services.PubSub;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using static SpotifyAPI.Web.Scopes;

namespace PhipseyyBot.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
    
[RequireUserPermission(GuildPermission.Administrator)]
[Group("spotify", "Spotify Commands")]
[EnabledInDm(false)]
public class SpotifyCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("set-account", "Adds your spotify config to the Server")]
    public async Task SetSpotifyCommand(string clientId = null, string clientSecret = null)
    {
        if (clientId == null || clientSecret == null)
        {
            await RespondAsync(embed: GetTutorialEmbed(), ephemeral: true);
            return;
        }

        var dbContext = DbService.GetDbContext();
        var creds = new BotCredsProvider().GetCreds();
        var uri = new Uri($"http://{creds.ServerIp}:5000/callback/spotify");
        var server = new EmbedIOAuthServer(uri, 5000);

        var (verifier, challenge) = PKCEUtil.GenerateCodes();

        await server.Start();
        server.AuthorizationCodeReceived += async (_, response) =>
        {
            await server.Stop();
            var token = await new OAuthClient().RequestToken(
                new PKCETokenRequest(clientId, response.Code, server.BaseUri, verifier)
            );

            await dbContext.SaveSpotifyConfigAsync(Context.Guild.Id, token, clientId, clientSecret);
            await FollowupAsync(
                text: "Settings saved!",
                ephemeral: true);
            
            PubSubService.AddSpotifyClient(Context.Guild);
            PubSubService.StartSpotifyForGuild(Context.Guild.Id);
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
        
        var embed = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = Context.Client.CurrentUser.Username,
                IconUrl = Context.Client.CurrentUser.GetAvatarUrl()
            },
            Title = "Spotify Login",
            Description = $"To connect your Spotify Account with your Server, login here: {request.ToUri()}",
            Timestamp = DateTime.Now,
            Color = Const.Spotify,
            Footer = new EmbedFooterBuilder
            {
                Text = "PhipseyyBot - Spotify"
            }
        };
        
        
        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    [SlashCommand("set-account", "Adds your spotify config to the Server")]
    
    private Embed GetTutorialEmbed()
    {
        var creds = new BotCredsProvider().GetCreds();
        
        var embed = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
                { Name = Context.Client.CurrentUser.Username, IconUrl = Context.Client.CurrentUser.GetAvatarUrl() },
            Color = Const.Spotify,
            Title = "How to connect your Spotify account:",
            Description = "Disclaimer: A Spotify **Premium** account is needed for this feature to work"
        };

        var dashboardLogin = new EmbedFieldBuilder
        {
            Name = "1. Login into your Spotify Dashboard",
            Value = "Login into your Spotify Dashboard here: \n" +
                    "https://developer.spotify.com/dashboard/login"
        };
        embed.AddField(dashboardLogin);

        var createApp = new EmbedFieldBuilder
        {
            Name = "2. Create your app",
            Value = "Click on 'Create app', chose a name and set an random description\n" +
                    "After that, welcome to your Dashboard!"
        };
        embed.AddField(createApp);

        var setupApplication = new EmbedFieldBuilder
        {
            Name = "3. Adjust the settings",
            Value = "Click on 'Edit Settings' \n" +
                    "Here u can see the option 'Redirect URIs':\n" +
                    $"Enter ``http://{creds.ServerIp}:5000/callback/spotify`` as your Redirect URI\n" +
                    "Click 'Add' and confirm everything with 'Save'"
        };
        embed.AddField(setupApplication);

        var grabCreds = new EmbedFieldBuilder
        {
            Name = "4. Done",
            Value = "You've set up your Application properly now!\n" +
                    "Now you're perfectly set to add your Spotify Account!\n" +
                    "Use ``/spotify set-account`` again with your Client-ID and Client-Secret"
        };
        embed.AddField(grabCreds);

        return embed.Build();
    }

}