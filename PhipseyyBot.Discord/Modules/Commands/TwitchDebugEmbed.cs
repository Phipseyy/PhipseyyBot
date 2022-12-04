using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using PhipseyyBot.Common.Modules;
using PhipseyyBot.Common.Services;
using TwitchLib.Api;

namespace PhipseyyBot.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireOwner]
public class TwitchDebugEmbed : InteractionModuleBase<SocketInteractionContext>
{
    [RequireOwner]
    [SlashCommand("twitch-debug", "[Owner] Debug twitch embed")]
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

        var usersData = api.Helix.Channels.GetChannelInformationAsync(id, creds.TwitchAccessToken).Result.Data.SingleOrDefault(x => x.BroadcasterId == id);
        var user = api.Helix.Search.SearchChannelsAsync(usersData!.BroadcasterName).Result.Channels.SingleOrDefault(x => x.DisplayName == usersData.BroadcasterName);
        var twitchData = new TwitchStreamData(user!.DisplayName,
            user.Id,
            user.Title,
            user.ThumbnailUrl,
            user.GameName,
            user.StartedAt);
        
        await RespondAsync("Done");
        await DeleteOriginalResponseAsync();
        await ReplyAsync(text: $"Hey @everyone! {twitchData.Username} is live again!", embed: twitchData.GetDiscordEmbed());

    }
}