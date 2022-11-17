using Discord;
using static System.DateTime;

namespace Phipseyy.Common.Services;

public class TwitchStreamData : ITwitchStreamData
{
    public string ChannelId { get; }
    public string Username { get; }
    public string Title { get; }
    public string UrlToProfilePicture { get; }
    public string UrlToPreview { get; }
    public string Game { get; }
    private DateTime? StartedAt { get; }

    public TwitchStreamData(string username, string channelId, string title, string urlToProfilePicture, string game,
        DateTime? userStartedAt)
    {
        Username = username;
        ChannelId = channelId;
        Title = title;
        UrlToProfilePicture = urlToProfilePicture;
        Game = game;
        UrlToPreview = $"https://static-cdn.jtvnw.net/previews-ttv/live_user_{Username.ToLower()}-1280x720.jpg";
        StartedAt = userStartedAt ?? Now;
    }

    public Embed GetDiscordEmbed()
    {
        var embed = new EmbedBuilder()
            .WithAuthor(Username, UrlToProfilePicture, $"https://twitch.tv/{Username}")
            .WithTitle(Title)
            .WithDescription($"Streaming: ``{Game}`` \nLive since: ``{StartedAt:HH:mm:ss} CEST``")
            .WithFooter(footer => footer.Text = "PhipseyyBot")
            .WithColor(Color.Red)
            .WithImageUrl(UrlToPreview)
            .WithThumbnailUrl(UrlToProfilePicture)
            .WithUrl($"https://twitch.tv/{Username}")
            .WithCurrentTimestamp();

        return embed.Build();
    }





}