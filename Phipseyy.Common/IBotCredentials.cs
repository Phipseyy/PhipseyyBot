namespace Phipseyy.Common;

public interface IBotCredentials
{
    string DiscordToken { get; }
    string DiscordStatus { get; }
    string TwitchUsername { get; }
    string TwitchId { get; }
    string TwitchAccessToken { get; }
    string TwitchRefreshToken { get; }
    string TwitchClientId { get; }
    string SpotifyClientId { get; }
    string SpotifyClientSecret { get; }
    
    string SpAccessToken { get; set; } 
    string SpTokenType { get; set; }
    int SpExpiresIn { get; set; }
    string SpScope { get; set; }
    string SpRefreshToken { get; set; }
    DateTime SpCreatedAt{ get; set; }
    bool SpIsExpired { get; set; }
}