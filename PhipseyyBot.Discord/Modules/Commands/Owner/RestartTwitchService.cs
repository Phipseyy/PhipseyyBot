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
        PubSubService.RestartService();
        //wait 5 seconds for the service to restart
        await Task.Delay(5000);
        if (PubSubService.IsConnected)
            await ReplyAsync("Service online again!");
        else
            await ReplyAsync("Service failed to restart! Check logs for more info.");
    }

}