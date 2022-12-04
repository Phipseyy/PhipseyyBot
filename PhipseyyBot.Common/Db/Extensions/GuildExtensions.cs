﻿#nullable disable
using Discord.WebSocket;
using PhipseyyBot.Common.Db.Models;

namespace PhipseyyBot.Common.Db.Extensions;

public static class GuildExtensions
{
    public static void AddGuildToDb(
        this PhipseyyDbContext context,
        ulong guildId,
        ulong logChannelId,
        ulong liveChannelId)
    {
        var config = new GuildConfig
        {
            GuildId = guildId,
            LogChannel = logChannelId,
            LiveChannel = liveChannelId
        };

        var guildConfig = context.GuildConfigs.FirstOrDefault(guildConfig => guildConfig.GuildId == guildId);
        if (guildConfig == null)
            context.GuildConfigs.Add(config);
        else
        {
            guildConfig.LogChannel = logChannelId;
            guildConfig.LiveChannel = liveChannelId;
        }
        
        context.SaveChangesAsync();
    }

    public static GuildConfig GetGuildConfig(this PhipseyyDbContext context, ulong guildId)
    {
        return context.GuildConfigs.FirstOrDefault(config => config.GuildId == guildId);
    }

    public static SocketTextChannel GetLogChannel(this PhipseyyDbContext context, SocketGuild guild)
    {
        var config = context.GuildConfigs.FirstOrDefault(config => config.GuildId == guild.Id);
        return config != null ? guild.TextChannels.FirstOrDefault(x => x.Id == config.LogChannel) : null;
    }
    
    public static SocketTextChannel GetLiveChannel(this PhipseyyDbContext context, SocketGuild guild)
    {
        var config = context.GuildConfigs.FirstOrDefault(config => config.GuildId == guild.Id);
        return config != null ? guild.TextChannels.FirstOrDefault(x => x.Id == config.LiveChannel) : null;
    }

}