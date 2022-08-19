namespace PhipseyyBot.Common;

public sealed class BotCredentials : IBotCredentials
{
    public string DiscordToken { get; }
    public string DiscordStatus { get; }
    public string TwitchUsername { get; }
    public string TwitchID { get; }
    public string TwitchAccesstoken { get; }
    public string TwitchRefreshToken { get; }
    public string TwitchClientID { get; }
    public string SpotifyClientID { get; }
    
    public BotCredentials()
    {
        DiscordToken = string.Empty;
        DiscordStatus = string.Empty;
        TwitchUsername = string.Empty;
        TwitchID = string.Empty;
        TwitchAccesstoken = string.Empty;
        TwitchRefreshToken = string.Empty;
        TwitchClientID = string.Empty;
        SpotifyClientID = string.Empty;
    }
}