using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;

namespace PhipseyyBot.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

public class Ping : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Pings the Bot")]
    public async Task PingCommand()
    {
        await RespondAsync($"Pong! {Context.Client.Latency}ms");
    }
}