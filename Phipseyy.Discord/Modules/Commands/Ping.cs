using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;

namespace Phipseyy.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

public class Bruh : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Test command to see if the bot responds")]
    public async Task BruhMoment()
    {
        await RespondAsync($"Pong! {Context.Client.Latency}ms");
    }
}