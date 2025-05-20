using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using PhipseyyBot.Common.Db.Models;
using PhipseyyBot.Common.Exceptions;
using PhipseyyBot.Common.Modules;
using PhipseyyBot.Common.Services;

namespace PhipseyyBot.Common.Db.Extensions;

public static class TwitchExtensions
{
    private static IQueryable<TwitchConfig> MainStreamFilter(this IQueryable<TwitchConfig> query) =>
        query.Where(config => config.MainStream);

    private static IQueryable<TwitchConfig> GuildFilter(this IQueryable<TwitchConfig> query, ulong guildId) =>
        query.Where(config => config.GuildId == guildId);

    private static IQueryable<TwitchConfig> UsernameFilter(this IQueryable<TwitchConfig> query, string username) =>
        query.Where(config => config.Username.ToLower() == username.ToLower());

    public static async Task<List<string>> GetAllStreamIdsAsync(
        this PhipseyyDbContext context) =>
        await context.TwitchConfigs
            .Select(config => config.ChannelId)
            .Distinct()
            .ToListAsync();

    public static async Task<List<TwitchConfig>> GetAllMainStreamIdsAsync(
        this PhipseyyDbContext context) =>
        await context.TwitchConfigs
            .MainStreamFilter()
            .ToListAsync();

    public static async Task<List<TwitchConfig>> GetGuildFollowedStreamsAsync(
        this PhipseyyDbContext context,
        ulong guildId) =>
        await context.TwitchConfigs
            .GuildFilter(guildId)
            .ToListAsync();

    public static async Task<TwitchConfig?> GetTwitchConfigAsync(
        this PhipseyyDbContext context,
        SocketGuild guild) =>
        await context.TwitchConfigs
            .GuildFilter(guild.Id)
            .FirstOrDefaultAsync();

    public static async Task<TwitchConfig?> GetTwitchConfigForStreamAsync(
        this PhipseyyDbContext context,
        SocketGuild guild,
        TwitchStreamData streamData) =>
        await context.TwitchConfigs
            .Where(config => config.GuildId == guild.Id && config.ChannelId == streamData.ChannelId)
            .FirstOrDefaultAsync();

    public static async Task<TwitchConfig?>
        GetMainStreamOfGuildAsync(this PhipseyyDbContext context, SocketGuild guild) =>
        await context.TwitchConfigs
            .GuildFilter(guild.Id)
            .MainStreamFilter()
            .FirstOrDefaultAsync();

    public static async Task<TwitchConfig?> GetMainStreamByChannelIdAsync(
        this PhipseyyDbContext context,
        string channelId
    ) =>
        await context.TwitchConfigs
            .Where(config => config.ChannelId == channelId)
            .MainStreamFilter()
            .FirstOrDefaultAsync();

    public static async Task FollowStreamAsync(
        this PhipseyyDbContext context,
        ulong guildId,
        string twitchName)
    {
        var twitchConfig = new TwitchConfig
        {
            Username = twitchName,
            ChannelId = TwitchConverter.GetTwitchIdFromName(twitchName),
            GuildId = guildId,
            MainStream = false,
            SpotifySr = ""
        };

        context.TwitchConfigs.Add(twitchConfig);
        await context.SaveChangesAsync();
    }

    public static async Task<bool> IsFollowingStreamAsync(
        this PhipseyyDbContext context,
        ulong guildId,
        string twitchName)
    {
        var stream = await context.TwitchConfigs
            .GuildFilter(guildId)
            .UsernameFilter(twitchName)
            .FirstOrDefaultAsync();

        return stream != null;
    }

    public static async Task UnfollowStreamAsync(
        this PhipseyyDbContext context,
        ulong guildId,
        string twitchName)
    {
        var channel = await context.TwitchConfigs
            .GuildFilter(guildId)
            .UsernameFilter(twitchName)
            .FirstOrDefaultAsync();

        if (channel != null)
        {
            context.Attach(channel);
            context.Remove(channel);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SetMainStreamAsync(
        this PhipseyyDbContext context,
        ulong guildId,
        string twitchName)
    {
        var guildConfig = await context.GuildConfigs.FirstOrDefaultAsync(g => g.GuildId == guildId);
        if (guildConfig == null)
        {
            guildConfig = new GuildConfig { GuildId = guildId };
            context.GuildConfigs.Add(guildConfig);
            await context.SaveChangesAsync();
        }

        var alreadyExists = await context.TwitchConfigs
            .Where(config => config.MainStream && config.GuildId != guildId)
            .UsernameFilter(twitchName)
            .AnyAsync();

        if (alreadyExists)
            throw new IsAlreadyMainStreamOnAnotherServerException(twitchName);

        var oldMainStream = await context.TwitchConfigs
            .GuildFilter(guildId)
            .MainStreamFilter()
            .FirstOrDefaultAsync();

        if (oldMainStream != null)
            oldMainStream.MainStream = false;

        var existingStream = await context.TwitchConfigs
            .GuildFilter(guildId)
            .UsernameFilter(twitchName)
            .FirstOrDefaultAsync();

        if (existingStream != null)
        {
            existingStream.MainStream = true;
        }
        else
        {
            var twitchConfig = new TwitchConfig
            {
                Username = twitchName,
                ChannelId = TwitchConverter.GetTwitchIdFromName(twitchName),
                GuildId = guildId,
                GuildConfig = guildConfig,
                MainStream = true
            };
            context.TwitchConfigs.Add(twitchConfig);
        }

        await context.SaveChangesAsync();
    }

    public static async Task DeleteTwitchConfigsForGuildAsync(
        this PhipseyyDbContext context,
        ulong guildId)
    {
        var twitchConfigs = await context.TwitchConfigs
            .GuildFilter(guildId)
            .ToListAsync();

        if (twitchConfigs.Any())
        {
            context.TwitchConfigs.RemoveRange(twitchConfigs);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SetSongRequestForStreamAsync(
        this PhipseyyDbContext context,
        SocketGuild guild,
        string rewardId)
    {
        var config = await context.GetMainStreamOfGuildAsync(guild);
        if (config != null)
        {
            config.SpotifySr = rewardId;
            await context.SaveChangesAsync();
        }
    }
}