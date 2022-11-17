using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Phipseyy.Discord.Services;

namespace Phipseyy.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[EnabledInDm(false)]
[RequireUserPermission(GuildPermission.Administrator)]
public class RestartTwitchService : InteractionModuleBase<SocketInteractionContext>
{
    [EnabledInDm(false)]
    [SlashCommand("restart-twitchservice", "Try to restart the currently running instance of the Twitch Service")]
    public async Task RestartTwitch()
    {
        await RespondAsync("Restarting service");
        await PubSub.RestartService();
        await ReplyAsync("Service restarted!");
    }

}