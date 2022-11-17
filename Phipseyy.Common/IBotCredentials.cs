namespace Phipseyy.Common;

public interface IBotCredentials
{
    string ServerIp { get; }
    string DiscordToken { get; }
    string DiscordStatus { get; set; }
    string TwitchUsername { get; }
    string TwitchAccessToken { get; }
    string TwitchRefreshToken { get; }
    string TwitchClientId { get; }
}