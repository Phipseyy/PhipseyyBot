using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using PhipseyyBot.Common;
using PhipseyyBot.Common.Db.Extensions;
using PhipseyyBot.Common.Embeds;
using PhipseyyBot.Common.Services;
using PhipseyyBot.Discord.Services.PubSub;

namespace PhipseyyBot.Discord.Modules.Commands;


[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireUserPermission(GuildPermission.Administrator)]
[Group("settings", "Settings for the Server")]
[EnabledInDm(false)]
public class SettingsCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("show", "Displays your current Server settings")]
    public async Task ShowSettingsCommand()
    {
        var dbContext = DbService.GetDbContext();
        var logChannel = dbContext.GetLogChannel(Context.Guild);
        var liveChannel = dbContext.GetLiveChannel(Context.Guild);
        var partnerChannel = dbContext.GetPartnerChannel(Context.Guild);
        var spotifyConfig = dbContext.GetSpotifyConfigFromGuild(Context.Guild.Id);
        var isSpotifyActiveEmoji = PubSubService.IsSpotifyActive(Context.Guild.Id)? "✅" : "❌";
        var streams = dbContext.GetListOfFollowedStreams(Context.Guild.Id);
        var isPubSubActiveEmoji = PubSubService.IsConnected? " ✅" : "❌";

        var embed = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = Context.Client.CurrentUser.Username,
                IconUrl = Context.Client.CurrentUser.GetAvatarUrl()
            },
            Title = $"Settings for {Context.Guild.Name}",
            Color = Const.Main,
            Footer = new EmbedFooterBuilder
            {
                Text = "PhipseyyBot - Settings"
            },
            Timestamp = DateTime.Now
        };


        if (logChannel != null || liveChannel != null || partnerChannel != null )  
            embed.AddField("Channels", 
                $"Log Channel: <#{logChannel!.Id}>\n" +
                $"Live Notifications Channel: <#{liveChannel!.Id}>\n" +
                $"Partner Notifications Channel: <#{partnerChannel!.Id}>");
        
        
        if (spotifyConfig != null)
            embed.AddField("Spotify",
                $"Is currently running: {isSpotifyActiveEmoji}\n" +
                $"Account: {PubSubService.GetSpotifyUsername(Context.Guild.Id)}");
        else
            embed.AddField("Spotify", "No account found, add your Spotify account with /spotify set-account" +
                                      "\n*Note that Spotify Premium is required to use the Song-Requests feature!*");

        embed.AddField("PubSub-Service", $"Online Status: {isPubSubActiveEmoji}"); 
        
        if (streams.Count > 0)
        {
            var field = new EmbedFieldBuilder
            {
                Name = "Followed Streams"
            };
            foreach (var currentStream in streams)
            {
                if (currentStream.MainStream)
                    field.Value += $"\n``{currentStream.Username}`` <- Main Streamer";    
                else
                    field.Value += $"\n``{currentStream.Username}``";
            }
            embed.AddField(field);
        }
        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }


    [Group("set", "Sets channels for the notifications")]

    public class SetSettings : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("main-channel", "Changes the main channel for live notifications")]
        public async Task SetLiveChannelCommand([ChannelTypes(ChannelType.News, ChannelType.Text)] IGuildChannel channel)
        {
            var dbService = DbService.GetDbContext();
            var guildConfig = dbService.GetGuildConfig(Context.Guild.Id);
            guildConfig.LiveChannel = channel.Id;

            await dbService.SaveChangesAsync();
            await RespondAsync(
                text: $"Changed the Live Notification channel to <#{channel.Id}>", ephemeral: true);
        }
        
        [SlashCommand("main-channel-noti", "Changes the message for main live notifications")]
        public async Task SetLiveChannelMessageCommand(string message)
        {
            var dbService = DbService.GetDbContext();
            await dbService.SetMainStreamNotification(Context.Guild.Id, message);

            await dbService.SaveChangesAsync();
            await RespondAsync(
                text: "Changed the Live Notification Message!", ephemeral: true);
        }

        [SlashCommand("partner-channel", "Changes the partner channel for live notifications")]
        public async Task SetPartnerChannelCommand(SocketTextChannel? channel = null)
        {
            var partnerChannel = channel?.Id ?? Context.Channel.Id;
        
            var dbService = DbService.GetDbContext();
            var guildConfig = dbService.GetGuildConfig(Context.Guild.Id);
            guildConfig.PartnerChannel = partnerChannel;

            await dbService.SaveChangesAsync();
            await RespondAsync(
                text: $"Changed the Partner Live Notification channel to <#{partnerChannel}>", ephemeral: true);
        }
        
        [SlashCommand("partner-channel-noti", "Changes the message for partner live notifications")]
        public async Task SetPartnerChannelMessageCommand(string message)
        {
            var dbService = DbService.GetDbContext();
            await dbService.SetPartnerStreamNotification(Context.Guild.Id, message);

            await dbService.SaveChangesAsync();
            await RespondAsync(
                text: "Changed the Live Notification Message!", ephemeral: true);
        }
        
    }


    [Group("reset", "Resets Settings")]
    public class ResetSettings : InteractionModuleBase<SocketInteractionContext>
    {

        [SlashCommand("spotify", "[WARNING] Deletes your spotify config")]
        public async Task ResetSpotifyCommand()
        {
            var dbContext = DbService.GetDbContext();
            PubSubService.DeleteSpotifyConfig(Context.Guild.Id);
            dbContext.DeleteSpotifyConfig(Context.Guild.Id);
            await RespondAsync(embed: SuccessEmbed.GetSuccessEmbed(Context.Client, "Spotify Settings Reset",
                "Your spotify data has been deleted from the database"), ephemeral: true);
        }

        [SlashCommand("twitch", "[WARNING] Deletes your twitch data")]
        public async Task ResetTwitchCommand()
        {
            var dbContext = DbService.GetDbContext();
            dbContext.DeleteTwitchConfig(Context.Guild.Id);
            await RespondAsync(embed: SuccessEmbed.GetSuccessEmbed(Context.Client,"Twitch Settings Reset",
                "Your twitch data has been deleted from the database"), ephemeral: true);
        }
    
        [SlashCommand("server", "[WARNING] Deletes ALL of your data and kicks the bot off the server\n")]
        public async Task ResetServerCommand()
        {
            var dbContext = DbService.GetDbContext();
            var guild = Context.Guild;
        
            dbContext.DeleteGuildConfig(guild.Id);
            dbContext.DeleteTwitchConfig(guild.Id);
            dbContext.DeleteSpotifyConfig(guild.Id);
            var invite = $"https://discord.com/api/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&permissions=8&scope=bot";
        
            await RespondAsync(embed: SuccessEmbed.GetSuccessEmbed(Context.Client,"Server Settings Reset",
                $"Your Server data has been deleted from the database.\nRe-invite me with the following link: {invite}"), 
                ephemeral: true);

            await Context.Guild.LeaveAsync();
        }
    }
}