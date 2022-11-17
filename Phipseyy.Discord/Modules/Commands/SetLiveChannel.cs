using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using Discord.WebSocket;
using Phipseyy.Common.Services;

namespace Phipseyy.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireOwner]
public class SetLiveChannel : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("set-live-channel", "Changes the channel for live notifications")]
    public async Task SetLiveChannelCommand(SocketTextChannel channel)
    {
        var dbService = DbService.GetDbContext();
        var guildConfig = dbService.GuildConfigs.FirstOrDefault(guild => guild.GuildId == Context.Guild.Id);
        guildConfig!.StreamNotificationChannel = channel.Id;

        await dbService.SaveChangesAsync();
        await RespondAsync(
            text: $"Changed the Live Notification channel to <#{channel.Id}>",
            ephemeral: true);
    }
}