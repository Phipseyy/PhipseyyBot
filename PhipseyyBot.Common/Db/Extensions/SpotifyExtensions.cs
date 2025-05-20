using Microsoft.EntityFrameworkCore;
using PhipseyyBot.Common.Db.Models;
using SpotifyAPI.Web;

namespace PhipseyyBot.Common.Db.Extensions;

public static class SpotifyExtensions
{
    private static IQueryable<SpotifyConfig> ByGuildId(this IQueryable<SpotifyConfig> query, ulong guildId) =>
        query.Where(config => config.GuildId == guildId);

    public static async Task<SpotifyConfig?> GetSpotifyConfigAsync(
        this PhipseyyDbContext context,
        ulong guildId) =>
        await context.SpotifyConfigs
            .ByGuildId(guildId)
            .FirstOrDefaultAsync();
    
    private static async Task<GuildConfig> EnsureGuildConfigExistsAsync(
        this PhipseyyDbContext context,
        ulong guildId)
    {
        var guildConfig = await context.GuildConfigs
            .FirstOrDefaultAsync(g => g.GuildId == guildId);

        if (guildConfig is not null) return guildConfig;
        
        guildConfig = new GuildConfig { GuildId = guildId };
        context.GuildConfigs.Add(guildConfig);
        await context.SaveChangesAsync();
        return guildConfig;
    }

    public static async Task SaveSpotifyConfigAsync(
        this PhipseyyDbContext context,
        ulong guildId,
        PKCETokenResponse token,
        string clientId,
        string clientSecret)
    {
        var guildConfig = await context.EnsureGuildConfigExistsAsync(guildId);

        var spotifyConfig = await context.SpotifyConfigs
            .ByGuildId(guildId)
            .FirstOrDefaultAsync();

        if (spotifyConfig == null)
        {
            spotifyConfig = new SpotifyConfig
            {
                GuildId = guildId,
                GuildConfigId = guildConfig.Id
            };
            context.SpotifyConfigs.Add(spotifyConfig);
        }

        spotifyConfig.SpotifyClientId = clientId;
        spotifyConfig.SpotifyClientSecret = clientSecret;
        spotifyConfig.Scope = token.Scope;
        spotifyConfig.AccessToken = token.AccessToken;
        spotifyConfig.CreatedAt = token.CreatedAt;
        spotifyConfig.ExpiresIn = token.ExpiresIn;
        spotifyConfig.RefreshToken = token.RefreshToken;
        spotifyConfig.TokenType = token.TokenType;

        await context.SaveChangesAsync();
    }

    public static async Task SaveSpotifyConfigAsync(
        this PhipseyyDbContext context,
        ulong guildId,
        AuthorizationCodeRefreshResponse token,
        string clientId,
        string clientSecret)
    {
        var guildConfig = await context.EnsureGuildConfigExistsAsync(guildId);

        var spotifyConfig = await context.SpotifyConfigs
            .ByGuildId(guildId)
            .FirstOrDefaultAsync();

        if (spotifyConfig == null)
        {
            spotifyConfig = new SpotifyConfig
            {
                GuildId = guildId,
                GuildConfigId = guildConfig.Id
            };
            context.SpotifyConfigs.Add(spotifyConfig);
        }

        spotifyConfig.SpotifyClientId = clientId;
        spotifyConfig.SpotifyClientSecret = clientSecret;
        spotifyConfig.Scope = token.Scope;
        spotifyConfig.AccessToken = token.AccessToken;
        spotifyConfig.CreatedAt = token.CreatedAt;
        spotifyConfig.ExpiresIn = token.ExpiresIn;
        spotifyConfig.RefreshToken = token.RefreshToken;
        spotifyConfig.TokenType = token.TokenType;

        await context.SaveChangesAsync();
    }

    public static async Task DeleteSpotifyConfigAsync(
        this PhipseyyDbContext context,
        ulong guildId)
    {
        var spotifyConfig = await context.SpotifyConfigs
            .ByGuildId(guildId)
            .FirstOrDefaultAsync();

        if (spotifyConfig != null)
        {
            context.SpotifyConfigs.Remove(spotifyConfig);
            await context.SaveChangesAsync();
        }
    }

    public static async Task<PKCETokenResponse?> GetSpotifyTokenAsync(
        this PhipseyyDbContext context,
        ulong guildId)
    {
        var spotifyConfig = await context.SpotifyConfigs
            .ByGuildId(guildId)
            .FirstOrDefaultAsync();

        if (spotifyConfig == null) return null;

        return new PKCETokenResponse
        {
            Scope = spotifyConfig.Scope,
            AccessToken = spotifyConfig.AccessToken,
            CreatedAt = spotifyConfig.CreatedAt,
            ExpiresIn = spotifyConfig.ExpiresIn,
            RefreshToken = spotifyConfig.RefreshToken,
            TokenType = spotifyConfig.TokenType
        };
    }
}