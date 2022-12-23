using Discord;
using Discord.WebSocket;

namespace PhipseyyBot.Common.Embeds;

public static class SuccessEmbed
{
    public static Embed GetSuccessEmbed(DiscordSocketClient client, string title, string message)
    {
        var embed = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = client.CurrentUser.Username,
                IconUrl = client.CurrentUser.GetAvatarUrl()
            },
            Title = title,
            Description = message,
            Color = Const.Success,
            Timestamp = DateTime.Now,
            Footer = new EmbedFooterBuilder
            {
                Text = "PhipseyyBot - Success"
            }
        };
        return embed.Build();
    }
}