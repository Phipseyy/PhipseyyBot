using Phipseyy.Common.Yml;

namespace Phipseyy.Common;

public sealed class BotCredentials : IBotCredentials
{
    [Comment("DiscordBot token. Do not share with anyone ever -> https://discordapp.com/developers/applications/")]
    public string DiscordToken { get; set; }
    
    [Comment("Text which gets displayed as the status of the Bot")]
    public string DiscordStatus { get; set; }
    public string TwitchUsername { get; set; }
    
    [Comment("ID of the Twitch-Channel -> https://chrome.google.com/webstore/detail/twitch-username-and-user/laonpoebfalkjijglbjbnkfndibbcoon")]
    public string TwitchId { get; set; }
    
    [Comment("Twitch Access -> https://twitchtokengenerator.com/")]
    public string TwitchAccessToken { get; set; }
    public string TwitchRefreshToken { get; set; }
    public string TwitchClientId { get; set; }

    [Comment("Spotify App Client ID -> https://developer.spotify.com/dashboard/")]
    public string SpotifyClientId { get; set; }
    public string SpotifyClientSecret { get; set; }

    [Comment("Spotify Token Data - DO NOT TOUCH / LEAVE EMPTY")]
    public string SpAccessToken { get; set; }
    public string SpTokenType { get; set; }
    public int SpExpiresIn { get; set; }
    public string SpScope { get; set; }
    public string SpRefreshToken { get; set; }
    public DateTime SpCreatedAt{ get; set; }
    public bool SpIsExpired { get; set; }
    

    public BotCredentials()
    {
        DiscordToken = string.Empty;
        DiscordStatus = string.Empty;
        TwitchUsername = string.Empty;
        TwitchId = string.Empty;
        TwitchAccessToken = string.Empty;
        TwitchRefreshToken = string.Empty;
        TwitchClientId = string.Empty;
        SpotifyClientId = string.Empty;
        SpotifyClientSecret = string.Empty;
        
        SpAccessToken = string.Empty;
        SpTokenType = string.Empty;
        SpExpiresIn = int.MinValue;
        SpScope = string.Empty;
        SpRefreshToken = string.Empty;
        SpCreatedAt = DateTime.MinValue;
        SpIsExpired = false;

    }
}