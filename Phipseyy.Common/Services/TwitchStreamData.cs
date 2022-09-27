using Discord;

namespace Phipseyy.Common.Services;

public class TwitchStreamData : ITwitchStreamData
{
    public string Username { get; }
    public string Title { get; }
    public string UrlToProfilePicture { get; }
    public string UrlToPreview { get; }
    public string Game { get; }

    public TwitchStreamData(string username, string title, string urlToProfilePicture, string game)
    {
        Username = username;
        Title = title;
        UrlToProfilePicture = urlToProfilePicture;
        Game = game;
        UrlToPreview = $"https://static-cdn.jtvnw.net/previews-ttv/live_user_{Username.ToLower()}-1280x720.jpg";
    }

    public EmbedBuilder GetDiscordEmbed()
    {
        var embed = new EmbedBuilder
        {
            Title = Title
        };
        embed.WithAuthor(Username)
            .WithFooter(footer => footer.Text = $"PhipseyyBot")
            .WithColor(Color.DarkRed)
            .WithImageUrl(UrlToPreview)
            .WithThumbnailUrl(UrlToProfilePicture)
            .WithUrl($"https://twitch.tv/{Username}")
            .WithCurrentTimestamp();

        return embed;
    }





}