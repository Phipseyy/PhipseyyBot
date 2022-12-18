using PhipseyyBot.Common.Yml;

namespace PhipseyyBot.Common;

public sealed class BotCredentials : IBotCredentials
{
    [Comment("Needed for Server functions e.g Authentication")]
    public string ServerIp { get; set; }

    [Comment("DiscordBot token. Do not share with anyone ever -> https://discordapp.com/developers/applications/")]
    public string DiscordToken { get; set; }

    [Comment("Text which gets displayed as the status of the Bot")]
    public string DiscordStatus { get; set; }

    public string TwitchUsername { get; set; }

    [Comment("Twitch Access -> https://twitchtokengenerator.com/")]
    public string TwitchAccessToken { get; set; }

    public string TwitchRefreshToken { get; set; }
    public string TwitchClientId { get; set; }

    [Comment("Twitch Application -> https://dev.twitch.tv/console/apps")]
    public string TwitchAppClientId { get; set; }

    public string TwitchAppClientSecret { get; set; }


    public BotCredentials()
    {
        ServerIp = string.Empty;
        DiscordToken = string.Empty;
        DiscordStatus = string.Empty;
        TwitchUsername = string.Empty;
        TwitchAccessToken = string.Empty;
        TwitchRefreshToken = string.Empty;
        TwitchClientId = string.Empty;
        TwitchAppClientId = string.Empty;
        TwitchAppClientSecret = string.Empty;
    }
}