namespace PhipseyyBot.Common;

public interface IBotCredentials
{
    string DiscordToken { get; }
    string DiscordStatus { get; }
    string TwitchUsername { get; }
    string TwitchId { get; }
    string TwitchAccesstoken { get; }
    string TwitchRefreshToken { get; }
    string TwitchClientId { get; }
    string SpotifyClientId { get; }
}