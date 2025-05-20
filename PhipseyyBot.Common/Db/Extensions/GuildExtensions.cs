using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using PhipseyyBot.Common.Db.Models;

namespace PhipseyyBot.Common.Db.Extensions;

public static class GuildExtensions
{
    private static IQueryable<GuildConfig> ByGuildId(this IQueryable<GuildConfig> query, ulong guildId) =>
        query.Where(config => config.GuildId == guildId);

    public static async Task AddOrUpdateGuildConfigAsync(
        this PhipseyyDbContext context,
        ulong guildId,
        ulong logChannelId,
        ulong liveChannelId,
        ulong partnerChannelId)
    {
        var guildConfig = await context.GuildConfigs
            .ByGuildId(guildId)
            .FirstOrDefaultAsync();

        if (guildConfig == null)
        {
            guildConfig = new GuildConfig
            {
                GuildId = guildId,
                LogChannel = logChannelId,
                LiveChannel = liveChannelId,
                PartnerChannel = partnerChannelId,
                MainStreamNotification = "Hey @everyone! {Username} is now live!",
                PartnerStreamNotification = "Hey @here! {Username} is now live!"
            };
            context.GuildConfigs.Add(guildConfig);
        }
        else
        {
            guildConfig.LogChannel = logChannelId;
            guildConfig.LiveChannel = liveChannelId;
            guildConfig.PartnerChannel = partnerChannelId;
        }

        await context.SaveChangesAsync();
    }

    public static async Task<GuildConfig?> GetGuildConfigAsync(
        this PhipseyyDbContext context,
        SocketGuild guild)
    {
        return await context.GuildConfigs
            .ByGuildId(guild.Id)
            .FirstOrDefaultAsync();
    }

    public static async Task<GuildConfig?> GetGuildConfigAsync(
        this PhipseyyDbContext context,
        ulong guildId)
    {
        return await context.GuildConfigs
            .ByGuildId(guildId)
            .FirstOrDefaultAsync();
    }

    public static async Task<SocketTextChannel?> GetLogChannelAsync(
        this PhipseyyDbContext context,
        SocketGuild guild)
    {
        var config = await context.GetGuildConfigAsync(guild);
        return config != null
            ? guild.TextChannels.FirstOrDefault(x => x.Id == config.LogChannel)
            : null;
    }

    public static async Task<SocketTextChannel?> GetLiveChannelAsync(
        this PhipseyyDbContext context,
        SocketGuild guild)
    {
        var config = await context.GetGuildConfigAsync(guild);
        return config != null
            ? guild.TextChannels.FirstOrDefault(x => x.Id == config.LiveChannel)
            : null;
    }

    public static async Task<SocketTextChannel?> GetPartnerChannelAsync(
        this PhipseyyDbContext context,
        SocketGuild guild)
    {
        var config = await context.GetGuildConfigAsync(guild);
        return config != null
            ? guild.TextChannels.FirstOrDefault(x => x.Id == config.PartnerChannel)
            : null;
    }

    public static async Task DeleteGuildConfigAsync(
        this PhipseyyDbContext context,
        ulong guildId)
    {
        var guildConfig = await context.GuildConfigs
            .ByGuildId(guildId).FirstOrDefaultAsync();

        if (guildConfig != null)
        {
            context.GuildConfigs.Remove(guildConfig);
            await context.SaveChangesAsync();
        }
    }

    public static async Task<string?> GetMainStreamNotificationAsync(
        this PhipseyyDbContext context,
        SocketGuild guild)
    {
        var config = await context.GetGuildConfigAsync(guild);
        return config?.MainStreamNotification;
    }

    public static async Task<string?> GetPartnerStreamNotificationAsync(
        this PhipseyyDbContext context,
        SocketGuild guild)
    {
        var config = await context.GetGuildConfigAsync(guild);
        return config?.PartnerStreamNotification;
    }

    public static async Task SetMainStreamNotificationAsync(
        this PhipseyyDbContext context,
        SocketGuild guild,
        string notification)
    {
        var config = await context.GetGuildConfigAsync(guild);
        if (config == null) return;

        config.MainStreamNotification = notification;
        await context.SaveChangesAsync();
    }

    public static async Task SetPartnerStreamNotificationAsync(
        this PhipseyyDbContext context,
        SocketGuild guild,
        string notification)
    {
        var config = await context.GetGuildConfigAsync(guild);
        if (config == null) return;

        config.PartnerStreamNotification = notification;
        await context.SaveChangesAsync();
    }

    public static async Task<bool> AreDebugNotificationsActiveAsync(
        this PhipseyyDbContext context,
        SocketGuild guild)
    {
        var config = await context.GetGuildConfigAsync(guild);
        return config.SendDebugMessages;
    }
}