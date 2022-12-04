using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Discord;
using Discord.Interactions;
using PhipseyyBot.Common.Db.Extensions;
using PhipseyyBot.Common.Services;
using TwitchLib.Api;
using TwitchLib.Api.Core.Enums;
using TwitchLib.PubSub;

namespace PhipseyyBot.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireUserPermission(GuildPermission.Administrator)]
public class CreateSongRequestReward : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("set-sr-reward", "Adds a Twitch Reward for the Song Requests")]
    public async Task SetSongRequestRewardCommand()
    {
        var dbContext = DbService.GetDbContext();
        var config = dbContext.GetMainStreamOfGuild(Context.Guild.Id);

        var _creds = new BotCredsProvider().GetCreds();
        
        var pubSub = new TwitchPubSub();

        pubSub.OnPubSubServiceConnected += (sender, args) =>
        {
            pubSub.ListenToRewards(config.ChannelId);
            pubSub.SendTopics();
            RespondAsync("Connected to PubSub");
        };

        pubSub.OnRewardRedeemed += (sender, args) =>
        {
            if (args.Message.Contains("spotify"))
                dbContext.SetSongRequestForStream(config.GuildId, args.RewardId.ToString());
            else
                RespondAsync("Send the Text 'Spotify' with your reward");

        };
        
        pubSub.Connect();
    }
}