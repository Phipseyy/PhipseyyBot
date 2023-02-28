namespace PhipseyyBot.Common.Services;

public static class TwitchStringHelper
{
    public static string ParseTwitchNotification(string message, TwitchStreamData twitchStreamData)
    {
        return message
            .Replace("{Username}", twitchStreamData.Username)
            .Replace("{Title}", twitchStreamData.Title)
            .Replace("{Game}", twitchStreamData.Game)
            .Replace("{Url}", $"https://twitch.tv/{twitchStreamData.Username}");
    }
}