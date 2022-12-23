using Discord;
using Discord.WebSocket;

namespace PhipseyyBot.Common.Embeds;

public static class AuthEmbed
{
    public static Embed GetAuthEmbed(this DiscordSocketClient client, string authLink)
    {
        var embed = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = client.CurrentUser.Username,
                IconUrl = client.CurrentUser.GetAvatarUrl()
            },
            Title = "Login needed",
            Description = "For this action is a token needed\n" +
                          "Please authenticate / login with this following link:\n" + authLink,
            Timestamp = DateTime.Now,
            Color = Const.Main,
            Footer = new EmbedFooterBuilder
            {
                Text = "PhipseyyBot - Auth"
            }
        };
        return embed.Build();
    }

}