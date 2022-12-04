using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using PhipseyyBot.Common.Services;

namespace PhipseyyBot.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireUserPermission(GuildPermission.Administrator)]
public class SetLiveChannel : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("set-live-channel", "Changes the channel for live notifications")]
    public async Task SetLiveChannelCommand(SocketTextChannel channel)
    {
        var dbService = DbService.GetDbContext();
        var guildConfig = dbService.GuildConfigs.FirstOrDefault(guild => guild.GuildId == Context.Guild.Id);
        guildConfig!.LiveChannel = channel.Id;

        await dbService.SaveChangesAsync();
        await RespondAsync(
            text: $"Changed the Live Notification channel to <#{channel.Id}>",
            ephemeral: true);
    }
}