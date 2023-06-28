#nullable disable
using Discord.WebSocket;
using PhipseyyBot.Common.Db.Models;

namespace PhipseyyBot.Common.Db.Extensions;

public static class GuildExtensions
{
    public static async Task AddGuildToDb(
        this PhipseyyDbContext context,
        ulong guildId,
        ulong logChannelId,
        ulong liveChannelId,
        ulong partnerChannelId)
    {
        var config = new GuildConfig
        {
            GuildId = guildId,
            LogChannel = logChannelId,
            LiveChannel = liveChannelId,
            PartnerChannel = partnerChannelId,
            MainStreamNotification = "Hey @everyone! {Username} is now live!",
            PartnerStreamNotification = "Hey @here! {Username} is now live!"
        };

        var guildConfig = context.GuildConfigs.FirstOrDefault(guildConfig => guildConfig.GuildId == guildId);
        if (guildConfig == null)
            context.GuildConfigs.Add(config);
        else
        {
            guildConfig.LogChannel = logChannelId;
            guildConfig.LiveChannel = liveChannelId;
            guildConfig.PartnerChannel = partnerChannelId;
        }
        
        await context.SaveChangesAsync();
    }

    public static GuildConfig GetGuildConfig(this PhipseyyDbContext context, SocketGuild guild)
    {
        return context.GuildConfigs.FirstOrDefault(config => config.GuildId == guild.Id);
    }

    public static SocketTextChannel GetLogChannel(this PhipseyyDbContext context, SocketGuild guild)
    {
        var config = GetGuildConfig(context, guild);
        return config != null ? guild.TextChannels.FirstOrDefault(x => x.Id == config.LogChannel) : null;
    }
    
    public static SocketTextChannel GetLiveChannel(this PhipseyyDbContext context, SocketGuild guild)
    {
        var config = GetGuildConfig(context, guild);
        return config != null ? guild.TextChannels.FirstOrDefault(x => x.Id == config.LiveChannel) : null;
    }
    
    public static SocketTextChannel GetPartnerChannel(this PhipseyyDbContext context, SocketGuild guild)
    {
        var config = GetGuildConfig(context, guild);
        return config != null ? guild.TextChannels.FirstOrDefault(x => x.Id == config.PartnerChannel) : null;
    }

    public static async void DeleteGuildConfig(
        this PhipseyyDbContext context,
        ulong guildId)
    {
        var guildEntries = context.GuildConfigs.FirstOrDefault(config => config.GuildId == guildId);
        if (guildEntries != null)
        {
            context.Attach(guildEntries);
            context.Remove(guildEntries);
        }
        await context.SaveChangesAsync();
    }
    
    public static async Task<string> GetMainStreamNotification(
        this PhipseyyDbContext context,
        SocketGuild guild)
    {
        var config = GetGuildConfig(context, guild);
        return config != null ? config.MainStreamNotification : null;
    }
    
    public static async Task<string> GetPartnerStreamNotification(
        this PhipseyyDbContext context,
        SocketGuild guild)
    {
        var config = GetGuildConfig(context, guild);
        return config != null ? config.PartnerStreamNotification : null;
    }
    
    public static async Task SetMainStreamNotification(
        this PhipseyyDbContext context,
        SocketGuild guild,
        string notification)
    {
        var config = GetGuildConfig(context, guild);
        if (config != null)
        {
            config.MainStreamNotification = notification;
            await context.SaveChangesAsync();
        }
    }
    
    public static async Task SetPartnerStreamNotification(
        this PhipseyyDbContext context,
        SocketGuild guild,
        string notification)
    {
        var config = GetGuildConfig(context, guild);
        if (config != null)
        {
            config.PartnerStreamNotification = notification;
            await context.SaveChangesAsync();
        }
    }

}