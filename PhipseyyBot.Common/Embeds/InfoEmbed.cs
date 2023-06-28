using Discord;
using Discord.WebSocket;

namespace PhipseyyBot.Common.Embeds;

public static class InfoEmbed
{
    public static Embed GetMessageInfoEmbed(this DiscordSocketClient client)
    {
        var embed = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = client.CurrentUser.Username,
                IconUrl = client.CurrentUser.GetAvatarUrl()
            },
            Title = "How to setup your Stream message",
            Description = "Use ``/settings set main-channel-noti`` or ``/settings set partner-channel-noti`` to change your live message for the notifications! \n" +
                "You can also use the following variables in your message: \n" +
                "``{Username}``: Username of the streamer \n" +
                "``{Game}``: Game for the Stream \n" +
                "``{Title}``: Title of the Stream \n" +
                "``{Url}``: Twitch Link for the Stream",
            Timestamp = DateTime.Now,
            Color = Const.Main,
            Footer = new EmbedFooterBuilder
            {
                Text = "PhipseyyBot - Info"
            }
        };
        return embed.Build();
    }
}