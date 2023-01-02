using Discord;
using Discord.WebSocket;

namespace PhipseyyBot.Common.Embeds;

public static class ErrorEmbed
{
    public static Embed GetErrorEmbed(this DiscordSocketClient client, string errorType, string message)
    {
        var embed = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = client.CurrentUser.Username,
                IconUrl = client.CurrentUser.GetAvatarUrl()
            },
            Title = $"ERROR - {errorType}",
            Description = message,
            Timestamp = DateTime.Now,
            Color = Const.Main,
            Footer = new EmbedFooterBuilder
            {
                Text = "PhipseyyBot - ERROR"
            }
        };
        
        
        return embed.Build();
    }
}