using PhipseyyBot.Common.Yml;

namespace PhipseyyBot.Common;

public sealed class BotCredentials : IBotCredentials
{
    [Comment("DiscordBot token. Do not share with anyone ever -> https://discordapp.com/developers/applications/")]
    public string DiscordToken { get; }
    
    [Comment("Text which gets displayed as the status of the Bot")]
    public string DiscordStatus { get; }
    public string TwitchUsername { get; }
    
    [Comment("ID of the Twitch-Channel -> https://chrome.google.com/webstore/detail/twitch-username-and-user/laonpoebfalkjijglbjbnkfndibbcoon")]
    public string TwitchId { get; }
    
    [Comment("Twitch Access -> https://twitchtokengenerator.com/")]
    public string TwitchAccesstoken { get; }
    public string TwitchRefreshToken { get; }
    public string TwitchClientId { get; }
    
    [Comment("Spotify App Client ID -> https://developer.spotify.com/dashboard/")]
    public string SpotifyClientId { get; }
    
    public BotCredentials()
    {
        DiscordToken = string.Empty;
        DiscordStatus = string.Empty;
        TwitchUsername = string.Empty;
        TwitchId = string.Empty;
        TwitchAccesstoken = string.Empty;
        TwitchRefreshToken = string.Empty;
        TwitchClientId = string.Empty;
        SpotifyClientId = string.Empty;
    }
}