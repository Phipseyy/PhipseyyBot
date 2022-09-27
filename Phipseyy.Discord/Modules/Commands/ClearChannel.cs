using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Commands;
using Discord.Interactions;

namespace Phipseyy.Discord.Modules.Commands;

[Name("clear-chanel")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[global::Discord.Interactions.RequireOwner]
public class ClearChannel : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("clear-channel", "Clears channel")]
    public async Task Clear()
    {
        var messages = await Context.Channel.GetMessagesAsync().FlattenAsync();
        await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
        await ReplyAsync("Cleared all channel messages which are not older than 14 days");
    }
}