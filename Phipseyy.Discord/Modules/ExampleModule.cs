using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using Discord.Interactions;

namespace Phipseyy.Discord.Modules;

[Name("Bruh-moment")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Bruh : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("bruh-moment", "Respond with an epic bruh-moment.")]
    public async Task BruhMoment()
    {
        await RespondAsync("Bruh");
    }
}