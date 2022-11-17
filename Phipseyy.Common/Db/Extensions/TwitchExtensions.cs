#nullable disable

using Phipseyy.Common.Db.Models;
using Phipseyy.Common.Modules;

namespace Phipseyy.Common.Db.Extensions;

public static class TwitchExtensions
{
    public static List<TwitchConfig> GetListOfFollowedStreams(
        this PhipseyyDbContext context,
        ulong guildId)
    {
        return context.TwitchConfigs.Local.Where((config, i) => config.GuildId == guildId).ToList();
    }

    public static TwitchConfig GetMainStreamOfGuild(
        this PhipseyyDbContext context,
        ulong guildId)
    {
        return context.TwitchConfigs.FirstOrDefault(config => config.GuildId == guildId && config.MainStream);
    }

    public static void FollowStream(
        this PhipseyyDbContext context,
        ulong guildId,
        string twitchName)
    {
        var twitchConfig = new TwitchConfig()
        {
            Username = twitchName,
            ChannelId = TwitchConverter.GetTwitchIdFromName(twitchName),
            GuildId = guildId,
            MainStream = false
        };
        context.TwitchConfigs.Add(twitchConfig);
        context.SaveChangesAsync();
    }
    
    public static void SetMainStream(
        this PhipseyyDbContext context,
        ulong guildId,
        string twitchName)
    {
        var twitchConfig = new TwitchConfig
        {
            Username = twitchName,
            ChannelId = TwitchConverter.GetTwitchIdFromName(twitchName),
            GuildId = guildId,
            MainStream = true,
            SpotifySr = ""
        };
        
        var oldMainStream = context.TwitchConfigs.FirstOrDefault(config => config.GuildId == guildId && config.MainStream);
        if (oldMainStream != null)
            oldMainStream.MainStream = false;

        var existingStream = context.TwitchConfigs.FirstOrDefault(config => config.GuildId == guildId && config.Username == twitchName);
        if (existingStream != null)
            existingStream.MainStream = true;
        else
            context.TwitchConfigs.Add(twitchConfig);

        context.SaveChangesAsync();
    }
}