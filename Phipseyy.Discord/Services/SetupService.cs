#nullable disable
using Discord;
using Discord.WebSocket;
using Phipseyy.Common.Db;
using Phipseyy.Common.Db.Extensions;

namespace Phipseyy.Discord.Services;

public static class SetupService
{
    private const string Owner = "Philted#3787";

    public static void InitializeChannels(PhipseyyDbContext context, SocketGuild socketGuild)
    {
        var logRequest = socketGuild.TextChannels.FirstOrDefault(x => x.Name.Contains("log"), null);
        var logChannel = logRequest ?? CreatePrivateTextChannelAsync(socketGuild, "log").Result;

        var liveRequest = socketGuild.TextChannels.FirstOrDefault(x => x.Name.Contains("stream") || x.Name.Contains("live"), null);
        var liveChannel = liveRequest ?? CreatePrivateTextChannelAsync(socketGuild, "stream-notifications").Result;
        
        context.AddGuildToDb(socketGuild.Id, logChannel.Id, liveChannel.Id);

        var welcomeMessage = new EmbedBuilder
        {
            Color = new Color(176, 11, 105),
            Title = "H0i",
            Description =
                $"Thank you for using PhipseyyBot! \nIf you encounter any bugs or errors, please contact {Owner}",
            ImageUrl = "https://media.giphy.com/media/8U8LDibipKRDq/giphy.gif",
            Footer = new EmbedFooterBuilder
            {
                Text = "PhipseyyBot"
            }
        };
        logChannel.SendMessageAsync(embed: welcomeMessage.Build());
    }
    
    private static async Task<SocketTextChannel> CreatePrivateTextChannelAsync(SocketGuild socketGuild,
        string channelName)
    {
        var channel = await socketGuild.CreateTextChannelAsync(channelName);
        await channel.AddPermissionOverwriteAsync(
            socketGuild.Roles.FirstOrDefault(role => role.Name == "@everyone"),
            OverwritePermissions.DenyAll(channel));

        return socketGuild.TextChannels.FirstOrDefault(guildChannel => guildChannel.Id == channel.Id);
    }
    
}