using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using PhipseyyBot.Common.Db.Extensions;
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
        var spotifyConfig = dbContext.GetSpotifyConfigFromGuild(Context.Guild.Id);
        var streams = dbContext.GetListOfFollowedStreams(Context.Guild.Id);

        var embed = new EmbedBuilder()
            .WithAuthor("PhipseyyBot")
            .WithColor(176, 11, 105)
            .WithFooter(footer => footer.Text = "PhipseyyBot")
            .WithCurrentTimestamp();

        if (logChannel != null || liveChannel != null)
            embed.AddField("Channels", 
                $"Log Channel: <#{logChannel!.Id}>\n" +
                $"Live Notifications Channel: <#{liveChannel!.Id}>");

        if (spotifyConfig != null)
            embed.AddField("Spotify",
                $"Is currently running: {PubSubService.IsSpotifyActive(Context.Guild.Id)}\n" +
                $"Account: {PubSubService.GetSpotifyUsername(Context.Guild.Id)}");
        else
            embed.AddField("Spotify", "No account found, add your Spotify account with /spotify set-account" +
                                      "\n*Note that Spotify Premium is required to use the Song-Requests feature!*");
        
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
        await RespondAsync(text:$"These are the current Settings for Server {Context.Guild.Name}", embed: embed.Build(), ephemeral: true);
    }
    
    
    [SlashCommand("set-live-channel", "Changes the channel for live notifications")]
    public async Task SetLiveChannelCommand(SocketTextChannel channel)
    {
        var dbService = DbService.GetDbContext();
        var guildConfig = dbService.GuildConfigs.FirstOrDefault(guild => guild.GuildId == Context.Guild.Id);
        guildConfig!.LiveChannel = channel.Id;

        await dbService.SaveChangesAsync();
        await RespondAsync(
            text: $"Changed the Live Notification channel to <#{channel.Id}>", ephemeral: true);
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
            await RespondAsync(text: "Your spotify data has been deleted from the database", ephemeral: true);
        }

        [SlashCommand("twitch", "[WARNING] Deletes your twitch data")]
        public async Task ResetTwitchCommand()
        {
            var dbContext = DbService.GetDbContext();
            dbContext.DeleteTwitchConfig(Context.Guild.Id);
            await RespondAsync(text: "Your twitch data has been deleted from the database", ephemeral: true);
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
        
            await RespondAsync(text: $"Your data has been deleted from the database.\nRe-invite me with the following link: {invite}", ephemeral: true);
            await Context.Guild.LeaveAsync();
        }
    }
}