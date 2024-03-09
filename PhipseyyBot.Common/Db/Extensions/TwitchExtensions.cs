#nullable disable
using Discord.WebSocket;
using PhipseyyBot.Common.Db.Models;
using PhipseyyBot.Common.Exceptions;
using PhipseyyBot.Common.Modules;
using PhipseyyBot.Common.Services;

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

    public static TwitchConfig GetTwitchConfig(
        this PhipseyyDbContext context,
        SocketGuild guild)
    {
        return context.TwitchConfigs.FirstOrDefault(config => config.GuildId == guild.Id);
    }

    public static TwitchConfig GetTwitchConfigForStream(
        this PhipseyyDbContext context,
        SocketGuild guild,
        TwitchStreamData streamData)
    {
        return context.TwitchConfigs.FirstOrDefault(config =>
            config.GuildId == guild.Id && config.ChannelId == streamData.ChannelId);
    }

    public static TwitchConfig GetMainStreamOfGuild(
        this PhipseyyDbContext context,
        SocketGuild guild)
    {
        return context.TwitchConfigs.FirstOrDefault(config => config.GuildId == guild.Id && config.MainStream);
    }

    public static TwitchConfig GetMainStreamGuildOfChannel(
        this PhipseyyDbContext context,
        string channelId)
    {
        return context.TwitchConfigs.FirstOrDefault(config => config.ChannelId == channelId && config.MainStream);
    }

    public static async Task FollowStream(
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

    public static async void UnfollowStream(
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

        await context.SaveChangesAsync();
    }

    public static async Task SetMainStream(
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
            throw new IsAlreadyMainStreamOnAnotherServerException(twitchName);

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

        await context.SaveChangesAsync();
    }

    public static async void DeleteTwitchConfig(
        this PhipseyyDbContext context,
        ulong guildId)
    {
        var twitchConfigs = context.TwitchConfigs.Where(config => config.GuildId == guildId);
        foreach (var config in twitchConfigs)
        {
            context.Attach(config);
            context.Remove(config);
        }

        await context.SaveChangesAsync();
    }


    public static async void SetSongRequestForStream(
        this PhipseyyDbContext context,
        SocketGuild guild,
        string rewardId)
    {
        var config = context.GetMainStreamOfGuild(guild);
        config.SpotifySr = rewardId;

        await context.SaveChangesAsync();
    }
}