using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Phipseyy.Discord.Services;

namespace Phipseyy.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireUserPermission(GuildPermission.Administrator)]
[RequireOwner]
public class RestartTwitchService : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("restart-twitch", "[Owner] Try to restart the currently running instance of the Twitch Service")]
    public async Task RestartTwitchServiceCommand()
    {
        await RespondAsync("Restarting service");
        await PubSubService.RestartService();
        await ReplyAsync("Service restarted!");
    }

}