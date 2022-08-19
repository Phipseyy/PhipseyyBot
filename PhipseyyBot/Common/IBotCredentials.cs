using PhipseyyBot.Common.Yml;

namespace PhipseyyBot.Common;

public interface IBotCredentials
{
    [Comment("DiscordBot token. Do not share with anyone ever -> https://discordapp.com/developers/applications/")]
    string DiscordToken { get; }
    
    
    [Comment("Text which gets displayed as the status of the Bot")]
    string DiscordStatus { get; }
    string TwitchUsername { get; }
    
    [Comment("ID of the Twitch-Channel -> https://chrome.google.com/webstore/detail/twitch-username-and-user/laonpoebfalkjijglbjbnkfndibbcoon")]
    string TwitchID { get; }
    
    [Comment("Twitch Access -> https://twitchtokengenerator.com/")]
    string TwitchAccesstoken { get; }
    string TwitchRefreshToken { get; }
    string TwitchClientID { get; }
    
    [Comment("Spotify App Client ID -> https://developer.spotify.com/dashboard/")]
    string SpotifyClientID { get; }
}