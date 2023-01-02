using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;

namespace PhipseyyBot.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireUserPermission(GuildPermission.Administrator)]
public class ClearChannel : InteractionModuleBase<SocketInteractionContext>
{
    [EnabledInDm(false)]
    [SlashCommand("clear-channel", "Clears channel - Messages shouldn't be older than 14 days")]
    public async Task ClearChannelCommand()
    {
        var messages = await Context.Channel.GetMessagesAsync().FlattenAsync();
        await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
        await RespondAsync("Cleared all channel messages which are not older than 14 days");
        await Task.Delay(2000);
        await DeleteOriginalResponseAsync();
    }
}