using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using PhipseyyBot.Common.Db.Extensions;
using PhipseyyBot.Common.Services;

namespace PhipseyyBot.Discord.Modules.Commands;


[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireUserPermission(GuildPermission.Administrator)]
[Group("channel", "Settings for the channels")]
[EnabledInDm(false)]
public class ChannelCommands: InteractionModuleBase<SocketInteractionContext>
{
    [Group("set", "Sets channels for the notifications")]
    public class SetSettings : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("log-channel", "Changes the channel for logging")]
        public async Task SetLogChannelCommand([ChannelTypes(ChannelType.News, ChannelType.Text)] IGuildChannel channel)
        {
            var dbService = DbService.GetDbContext();
            var guildConfig = dbService.GetGuildConfig(Context.Guild);
            guildConfig.LogChannel = channel.Id;

            await dbService.SaveChangesAsync();
            await RespondAsync(
                text: $"Changed the Log channel to <#{channel.Id}>", ephemeral: true);
        }
        
        
        [SlashCommand("main-channel", "Changes the main channel for live notifications")]
        public async Task SetLiveChannelCommand([ChannelTypes(ChannelType.News, ChannelType.Text)] IGuildChannel channel)
        {
            var dbService = DbService.GetDbContext();
            var guildConfig = dbService.GetGuildConfig(Context.Guild);
            guildConfig.LiveChannel = channel.Id;

            await dbService.SaveChangesAsync();
            await RespondAsync(
                text: $"Changed the Live Notification channel to <#{channel.Id}>", ephemeral: true);
        }

        [SlashCommand("partner-channel", "Changes the partner channel for live notifications")]
        public async Task SetPartnerChannelCommand([ChannelTypes(ChannelType.News, ChannelType.Text)] IGuildChannel channel)
        {
            var dbService = DbService.GetDbContext();
            var guildConfig = dbService.GetGuildConfig(Context.Guild);
            guildConfig.PartnerChannel = channel.Id;

            await dbService.SaveChangesAsync();
            await RespondAsync(
                text: $"Changed the Partner Live Notification channel to <#{channel.Id}>", ephemeral: true);
        }

    }
    
    [RequireUserPermission(GuildPermission.Administrator)]
    public class ClearChannel : InteractionModuleBase<SocketInteractionContext>
    {
        [EnabledInDm(false)]
        [SlashCommand("clear", "Clears channel - Messages shouldn't be older than 14 days")]
        public async Task ClearChannelCommand()
        {
            var messages = await Context.Channel.GetMessagesAsync().FlattenAsync();
            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
            await RespondAsync("Cleared all channel messages which are not older than 14 days");
            await Task.Delay(2000);
            await DeleteOriginalResponseAsync();
        }
    }
    
}