#nullable disable
using Phipseyy.Common.Db.Models;

namespace Phipseyy.Common.Db.Extensions;

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
            StreamNotificationChannel = liveChannelId
        };

        var guildConfig = context.GuildConfigs.FirstOrDefault(guildConfig => guildConfig.GuildId == guildId);
        if (guildConfig == null)
            context.GuildConfigs.Add(config);
        else
        {
            guildConfig.LogChannel = logChannelId;
            guildConfig.StreamNotificationChannel = liveChannelId;
        }
        
        context.SaveChangesAsync();
    }

    public static GuildConfig GetGuildConfig(this PhipseyyDbContext context, ulong guildId)
    {
        return context.GuildConfigs.FirstOrDefault(config => config.GuildId == guildId);
    }
}