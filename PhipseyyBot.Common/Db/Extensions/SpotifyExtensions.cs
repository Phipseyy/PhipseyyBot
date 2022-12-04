#nullable disable
using PhipseyyBot.Common.Db.Models;
using SpotifyAPI.Web;

namespace PhipseyyBot.Common.Db.Extensions;

public static class SpotifyExtensions
{
    public static void SetSpotifyDataToDb(
        this PhipseyyDbContext context,
        ulong guildId,
        PKCETokenResponse token,
        string clientId,
        string clientSecret)
    {
        var spotifyConfig = new SpotifyConfig
        {
            GuildId = guildId,
            SpotifyClientId = clientId,
            SpotifyClientSecret = clientSecret,
            Scope = token.Scope,
            AccessToken = token.AccessToken,
            CreatedAt = token.CreatedAt,
            ExpiresIn = token.ExpiresIn,
            RefreshToken = token.RefreshToken,
            TokenType = token.TokenType
        };

        var oldSpotifyConfig = context.SpotifyConfigs.FirstOrDefault(config => config.GuildId == guildId);
        if (oldSpotifyConfig == null)
        {
            context.SpotifyConfigs.Add(spotifyConfig);
        }
        else
        {
            oldSpotifyConfig.SpotifyClientId = spotifyConfig.SpotifyClientId;
            oldSpotifyConfig.SpotifyClientSecret = spotifyConfig.SpotifyClientSecret;
            oldSpotifyConfig.Scope = token.Scope;
            oldSpotifyConfig.AccessToken = token.AccessToken;
            oldSpotifyConfig.CreatedAt = token.CreatedAt;
            oldSpotifyConfig.ExpiresIn = token.ExpiresIn;
            oldSpotifyConfig.RefreshToken = token.RefreshToken;
            oldSpotifyConfig.TokenType = token.TokenType;
        }

        context.SaveChangesAsync();
    }

    public static SpotifyConfig GetSpotifyConfigFromGuild(this PhipseyyDbContext context, ulong guildId)
    {
        return context.SpotifyConfigs.FirstOrDefault(config => config.GuildId == guildId);
    }


    public static PKCETokenResponse GetSpotifyToken(
        this PhipseyyDbContext context,
        ulong guildId)
    {
        var spotifyConfig = context.SpotifyConfigs.FirstOrDefault(config => config.GuildId == guildId);

        if (spotifyConfig == null) return null;
        var token = new PKCETokenResponse
        {
            Scope = spotifyConfig.Scope,
            AccessToken = spotifyConfig.AccessToken,
            CreatedAt = spotifyConfig.CreatedAt,
            ExpiresIn = spotifyConfig.ExpiresIn,
            RefreshToken = spotifyConfig.RefreshToken,
            TokenType = spotifyConfig.TokenType
        };
        return token;

    }
    
    
    public static void SetSpotifyDataToDb(
        this PhipseyyDbContext context,
        ulong guildId,
        AuthorizationCodeRefreshResponse token,
        string clientId,
        string clientSecret)
    {
        var spotifyConfig = new SpotifyConfig
        {
            GuildId = guildId,
            SpotifyClientId = clientId,
            SpotifyClientSecret = clientSecret,
            Scope = token.Scope,
            AccessToken = token.AccessToken,
            CreatedAt = token.CreatedAt,
            ExpiresIn = token.ExpiresIn,
            RefreshToken = token.RefreshToken,
            TokenType = token.TokenType
        };

        var oldSpotifyConfig = context.SpotifyConfigs.FirstOrDefault(config => config.GuildId == guildId);
        if (oldSpotifyConfig == null)
        {
            context.SpotifyConfigs.Add(spotifyConfig);
        }
        else
        {
            oldSpotifyConfig.SpotifyClientId = spotifyConfig.SpotifyClientId;
            oldSpotifyConfig.SpotifyClientSecret = spotifyConfig.SpotifyClientSecret;
            oldSpotifyConfig.Scope = token.Scope;
            oldSpotifyConfig.AccessToken = token.AccessToken;
            oldSpotifyConfig.CreatedAt = token.CreatedAt;
            oldSpotifyConfig.ExpiresIn = token.ExpiresIn;
            oldSpotifyConfig.RefreshToken = token.RefreshToken;
            oldSpotifyConfig.TokenType = token.TokenType;
        }

        context.SaveChangesAsync();
    }

}