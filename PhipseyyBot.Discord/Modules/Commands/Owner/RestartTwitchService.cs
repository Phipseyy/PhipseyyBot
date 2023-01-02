using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using PhipseyyBot.Discord.Services.PubSub;

namespace PhipseyyBot.Discord.Modules.Commands.Owner;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

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