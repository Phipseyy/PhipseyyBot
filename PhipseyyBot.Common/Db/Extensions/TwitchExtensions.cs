#nullable disable
using PhipseyyBot.Common.Db.Models;
using PhipseyyBot.Common.Modules;

namespace PhipseyyBot.Common.Db.Extensions;

public static class TwitchExtensions
{
    public static List<string> GetListOfAllStreamIds(
        this PhipseyyDbContext context)
    {
        return context.TwitchConfigs.Select(config => config.ChannelId).Distinct().ToList();
    }

    public static List<TwitchConfig> GetListOfAllMainStreams(
        this PhipseyyDbContext context)
    {
        return context.TwitchConfigs.Where(config => config.MainStream).Distinct().ToList();
    }

    public static List<TwitchConfig> GetListOfFollowedStreams(
        this PhipseyyDbContext context,
        ulong guildId)
    {
        return context.TwitchConfigs.Where(config => config.GuildId == guildId).ToList();
    }

    public static TwitchConfig GetMainStreamOfGuild(
        this PhipseyyDbContext context,
        ulong guildId)
    {
        return context.TwitchConfigs.FirstOrDefault(config => config.GuildId == guildId && config.MainStream);
    }

    public static TwitchConfig GetMainStreamOfChannel(
        this PhipseyyDbContext context,
        string channelId)
    {
        return context.TwitchConfigs.FirstOrDefault(config => config.ChannelId == channelId && config.MainStream);
    }

    public static void FollowStream(
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
        context.SaveChangesAsync();
    }

    public static bool IsFollowingStream(
        this PhipseyyDbContext context,
        ulong guildId,
        string twitchName)
    {
        var stream =
            context.TwitchConfigs.FirstOrDefault(config =>
                config.GuildId == guildId && config.Username.ToLower().Equals(twitchName.ToLower()));

        return stream != null;
    }

    public static void UnfollowStream(
        this PhipseyyDbContext context,
        ulong guildId,
        string twitchName)
    {
        var channel = context.TwitchConfigs.FirstOrDefault(config =>
            config.GuildId == guildId && config.Username.ToLower().Equals(twitchName.ToLower()));
        if (channel != null)
        {
            context.Attach(channel);
            context.Remove(channel);
        }

        context.SaveChangesAsync();
    }

    public static bool SetMainStream(
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

        var alreadyExists =
            context.TwitchConfigs.FirstOrDefault(config =>
                config.MainStream == true && config.GuildId != guildId &&
                config.Username.ToLower().Equals(twitchName.ToLower()));
        if (alreadyExists != null)
            return false;

        var oldMainStream =
            context.TwitchConfigs.FirstOrDefault(config => config.GuildId == guildId && config.MainStream);
        if (oldMainStream != null)
            oldMainStream.MainStream = false;

        var existingStream =
            context.TwitchConfigs.FirstOrDefault(config =>
                config.GuildId == guildId && config.Username.ToLower().Equals(twitchName.ToLower()));
        if (existingStream != null)
            existingStream.MainStream = true;
        else
            context.TwitchConfigs.Add(twitchConfig);

        context.SaveChangesAsync();
        return true;
    }

    public static void DeleteTwitchConfig(
        this PhipseyyDbContext context,
        ulong guildId)
    {
        var twitchConfigs = context.TwitchConfigs.Where(config => config.GuildId == guildId);
        foreach (var config in twitchConfigs)
        {
            context.Attach(config);
            context.Remove(config);
        }

        context.SaveChangesAsync();
    }


    public static void SetSongRequestForStream(
        this PhipseyyDbContext context,
        ulong guildId,
        string rewardId)
    {
        var config = context.GetMainStreamOfGuild(guildId);
        config.SpotifySr = rewardId;

        context.SaveChangesAsync();
    }
}