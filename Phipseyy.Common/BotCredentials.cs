using Phipseyy.Common.Yml;

namespace Phipseyy.Common;

public sealed class BotCredentials : IBotCredentials
{
    [Comment("Needed for the Server functions e.g Authentication")]
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

    public BotCredentials()
    {
        ServerIp = string.Empty;
        DiscordToken = string.Empty;
        DiscordStatus = string.Empty;
        TwitchUsername = string.Empty;
        TwitchAccessToken = string.Empty;
        TwitchRefreshToken = string.Empty;
        TwitchClientId = string.Empty;
    }
}