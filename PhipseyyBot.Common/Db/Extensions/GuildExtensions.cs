#nullable disable
using Discord.WebSocket;
using PhipseyyBot.Common.Db.Models;

namespace PhipseyyBot.Common.Db.Extensions;

public static class GuildExtensions
{
    public static void AddGuildToDb(
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
            PartnerChannel = partnerChannelId
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
        
        context.SaveChangesAsync();
    }

    public static GuildConfig GetGuildConfig(this PhipseyyDbContext context, ulong guildId)
    {
        return context.GuildConfigs.FirstOrDefault(config => config.GuildId == guildId);
    }

    public static SocketTextChannel GetLogChannel(this PhipseyyDbContext context, SocketGuild guild)
    {
        var config = GetGuildConfig(context, guild.Id);
        return config != null ? guild.TextChannels.FirstOrDefault(x => x.Id == config.LogChannel) : null;
    }
    
    public static SocketTextChannel GetLiveChannel(this PhipseyyDbContext context, SocketGuild guild)
    {
        var config = GetGuildConfig(context, guild.Id);
        return config != null ? guild.TextChannels.FirstOrDefault(x => x.Id == config.LiveChannel) : null;
    }
    
    public static SocketTextChannel GetPartnerChannel(this PhipseyyDbContext context, SocketGuild guild)
    {
        var config = GetGuildConfig(context, guild.Id);
        return config != null ? guild.TextChannels.FirstOrDefault(x => x.Id == config.PartnerChannel) : null;
    }

    public static void DeleteGuildConfig(
        this PhipseyyDbContext context,
        ulong guildId)
    {
        var guildEntries = context.GuildConfigs.FirstOrDefault(config => config.GuildId == guildId);
        if (guildEntries != null)
        {
            context.Attach(guildEntries);
            context.Remove(guildEntries);
        }
        context.SaveChangesAsync();
    }

}