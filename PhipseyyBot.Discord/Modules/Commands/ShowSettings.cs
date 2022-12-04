using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using PhipseyyBot.Common.Db.Extensions;
using PhipseyyBot.Common.Services;
using PhipseyyBot.Discord.Services.PubSub;


namespace PhipseyyBot.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireUserPermission(GuildPermission.Administrator)]
public class ShowSettings : InteractionModuleBase<SocketInteractionContext>
{

    [SlashCommand("show-settings", "Displays your current Server settings")]
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
            embed.AddField("Spotify", "No account found, add your Spotify account with /add-spotify" +
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


}