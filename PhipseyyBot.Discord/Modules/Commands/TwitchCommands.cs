using System.Diagnostics.CodeAnalysis;
using System.Net;
using Discord;
using Discord.Interactions;
using NHttp;
using PhipseyyBot.Common.Db.Extensions;
using PhipseyyBot.Common.Modules;
using PhipseyyBot.Common.Services;
using PhipseyyBot.Discord.Services.PubSub;
using TwitchLib.Api;
using TwitchLib.Api.Core.Enums;


namespace PhipseyyBot.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[RequireUserPermission(GuildPermission.Administrator)]
[Group("twitch", "Twitch Commands")]
[EnabledInDm(false)]
public class TwitchCommands : InteractionModuleBase<SocketInteractionContext>
{
    private string tclient = "8x9s0y67yonhax5g3keox0vcyjnvxo";
    private string tstuff = "czfh6j2hbrw15nynx7kej003t2srec";

    [SlashCommand("follow", "Activates notifications for a certain Twitch Stream")]
    public async Task FollowStreamCommand(string twitchName)
    {
        var dbContext = DbService.GetDbContext();
        dbContext.FollowStream(Context.Guild.Id, twitchName);
        PubSubService.FollowStream(twitchName);
        await RespondAsync($"This server will now receive notifications when {twitchName} is live");
    }

    [SlashCommand("main-streamer", "Sets the main Stream for this Server")]
    public async Task SetStreamCommand(string twitchName)
    {
        var dbContext = DbService.GetDbContext();
        if (dbContext.SetMainStream(Context.Guild.Id, twitchName))
            await RespondAsync($"Main stream has been set to {twitchName}");
        else
            await RespondAsync($"{twitchName} is already the Main Stream from another Server");
    }

    [RequireOwner]
    [SlashCommand("debug", "[Owner] Debug twitch embed")]
    public async Task TwitchDebugEmbedCommand(string name)
    {
        var creds = new BotCredsProvider().GetCreds();
        var id = TwitchConverter.GetTwitchIdFromName(name);

        var api = new TwitchAPI
        {
            Settings =
            {
                AccessToken = creds.TwitchAccessToken,
                ClientId = creds.TwitchClientId
            }
        };

        var usersData = api.Helix.Channels.GetChannelInformationAsync(id, creds.TwitchAccessToken).Result.Data
            .SingleOrDefault(x => x.BroadcasterId == id);
        var user = api.Helix.Search.SearchChannelsAsync(usersData!.BroadcasterName).Result.Channels
            .SingleOrDefault(x => x.DisplayName == usersData.BroadcasterName);
        var twitchData = new TwitchStreamData(user!.DisplayName,
            user.Id,
            user.Title,
            user.ThumbnailUrl,
            user.GameName,
            user.StartedAt);

        await RespondAsync("Done");
        await DeleteOriginalResponseAsync();
        await ReplyAsync(text: $"Hey @everyone! {twitchData.Username} is live again!",
            embed: twitchData.GetDiscordEmbed());
    }


    [SlashCommand("set-sr-reward", "Adds a Twitch Reward for the Song Requests")]
    public async Task SetSongRequestRewardCommand()
    {
        var dbContext = DbService.GetDbContext();
        var config = dbContext.GetMainStreamOfGuild(Context.Guild.Id);
        
        var api = new TwitchAPI
        {
            Settings =
            {
                ClientId = tclient,
                Secret = tstuff,
                Scopes = new List<AuthScopes>
                    { AuthScopes.Helix_Channel_Manage_Redemptions, AuthScopes.Helix_Channel_Read_Redemptions }
            }
        };


        var server = new HttpServer();
        server.EndPoint = new IPEndPoint(IPAddress.Loopback, 80);
        

        await RespondAsync(text: "Gimme access plz\n" + "https://id.twitch.tv/oauth2/authorize?" +
                           $"client_id={tclient}&" +
                           "redirect_uri=http://localhost&" +
                           "response_type=code&" +
                           $"scope=channel:manage:redemptions",
                            ephemeral: true);

        server.RequestReceived += async (_, args) =>
        {
            if (!args.Request.QueryString.AllKeys.Any("code".Contains!)) return;
            var authCode = args.Request.QueryString["code"];
            var authToken = await api.Auth.GetAccessTokenFromCodeAsync(authCode, tstuff, "http://localhost");
            var rewards = await api.Helix.ChannelPoints.GetCustomRewardAsync(config.ChannelId, accessToken: authToken.AccessToken);

            foreach (var reward in rewards.Data)
            {
                if (!reward.Title.ToLower().Contains("spotify")) continue;
                dbContext.SetSongRequestForStream(Context.Guild.Id, reward.Id);
                await ReplyAsync(text: $"Song Request for the Reward ''{reward.Title}'' has been set!");
            }
        };
        
        server.Start();
    }
}