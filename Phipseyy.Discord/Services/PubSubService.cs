#nullable disable
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Phipseyy.Common;
using Phipseyy.Common.Db;
using Phipseyy.Common.Db.Extensions;
using Phipseyy.Common.Modules;
using Phipseyy.Common.Services;
using Phipseyy.Spotify;
using Serilog;
using TwitchLib.Api;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using static System.DateTime;

namespace Phipseyy.Discord.Services;

public class PubSubService
{
    private static Dictionary<SocketGuild, SpotifyBot> _spotifyClients;
    private static PhipseyyDbContext _dbContext;
    private static DiscordBot _discordBot;
    private static TwitchPubSub _pubSub;
    private static IBotCredentials _creds;
    
    
    public PubSubService(DiscordBot discordBot, IServiceProvider serviceProvider)
    {
        _dbContext = serviceProvider.GetRequiredService<PhipseyyDbContext>();
        _discordBot = discordBot;
        _spotifyClients = new Dictionary<SocketGuild, SpotifyBot>();
        _pubSub = new TwitchPubSub();
        _creds = new BotCredsProvider().GetCreds();
    }

    public async Task InitializePubSub()
    {
        _pubSub.OnPubSubServiceConnected += PubSub_OnServiceConnected;
        _pubSub.OnPubSubServiceClosed += PubSubOn_OnPubSubServiceClosed;
        _pubSub.OnStreamUp += PubSub_OnStreamUp;
        _pubSub.OnChannelPointsRewardRedeemed += PubSub_OnChannelPointsRewardRedeemed;
        _pubSub.OnLog += PubSub_OnLog;
        _pubSub.Connect();
        
        await Task.Delay(-1);
    }
    
    /* --- Helpers --- */
    private static void LogTwitchPubSub(string message)
    {
        if(!message.Contains("PING") || !message.Contains("PONG"))
            Log.Information($"[TwitchPubSub] {Now:HH:mm:ss} {message}");
    }
    
    public static Task RestartService()
    {
        _pubSub.Disconnect();
        _pubSub.Connect();
        return Task.CompletedTask;
    }

    /* --- PubSub Events --- */
    private static void PubSub_OnLog(object sender, OnLogArgs e)
        => LogTwitchPubSub(e.Data);

    private void PubSubOn_OnPubSubServiceClosed(object sender, EventArgs e)
    {
        LogTwitchPubSub("PubSubService got closed! Restarting...");
        try
        {
            _pubSub.Connect();
        }
        catch (Exception exception)
        {
            LogTwitchPubSub(exception.Message);
            throw;
        }
    }

    private void PubSub_OnChannelPointsRewardRedeemed(object sender, OnChannelPointsRewardRedeemedArgs e)
    {
        var currentConfig = _dbContext.GetMainStreamOfChannel(e.ChannelId);
        
        if (e.RewardRedeemed.Redemption.Reward.Id.Equals(currentConfig.SpotifySr))
        {
            LogTwitchPubSub($"Song Request: {e.RewardRedeemed.Redemption.UserInput}");
            var client = _spotifyClients.FirstOrDefault(pair => pair.Key.Id == currentConfig.GuildId).Value;
            client.AddSongToQueue(e.RewardRedeemed.Redemption.UserInput);
        }
    }

    private async void PubSub_OnStreamUp(object sender, OnStreamUpArgs e)
    {
        try
        {
            LogTwitchPubSub("Stream is up now!");
            var api = new TwitchAPI
            {
                Settings =
                {
                    AccessToken = _creds.TwitchAccessToken,
                    ClientId = _creds.TwitchClientId
                }
            };

            var usersData = api.Helix.Channels.GetChannelInformationAsync(e.ChannelId, _creds.TwitchAccessToken).Result.Data.SingleOrDefault(x => x.BroadcasterId == e.ChannelId);
            var user = api.Helix.Search.SearchChannelsAsync(usersData!.BroadcasterName).Result.Channels.SingleOrDefault(x => x.DisplayName == usersData.BroadcasterName);
            var twitchData = new TwitchStreamData(user!.DisplayName,
                user!.Id,
                user.Title,
                user.ThumbnailUrl,
                user.GameName,
                user.StartedAt);

            await _discordBot.SendStreamNotification(twitchData);
        }
        catch (Exception exception)
        {
            LogTwitchPubSub(exception.Message);
            _discordBot.SendGlobalLogMessage($"OnStreamUp: {exception.Message}");
        }
    }

    private void PubSub_OnServiceConnected(object sender, EventArgs e)
    {
        LogTwitchPubSub("---PubSub Connected!---");
        _discordBot.SendGlobalLogMessage("PubSub Service online!");

        var streams = _dbContext.GetListOfAllStreams();

        foreach (var config in streams)
            _pubSub.ListenToVideoPlayback(config.ChannelId);

        var mainStreams = _dbContext.GetListOfAllMainStreams();
        foreach (var mainStreamConfig in mainStreams)
            _pubSub.ListenToChannelPoints(mainStreamConfig.ChannelId);

        _pubSub.ListenToVideoPlayback(TwitchConverter.GetTwitchIdFromName(_creds.TwitchUsername));
        _pubSub.ListenToChannelPoints(TwitchConverter.GetTwitchIdFromName(_creds.TwitchUsername));
        _pubSub.SendTopics(_creds.TwitchAccessToken);
    }

    public static void FollowStream(string twitchUser)
    {
        _pubSub.ListenToVideoPlayback(TwitchConverter.GetTwitchIdFromName(twitchUser));
        _pubSub.SendTopics(_creds.TwitchAccessToken);
    }


    public static void AddSpotifyClientCmd(SocketGuild guild)
    {
        var spotifyBot = new SpotifyBot(guild.Id);

        _spotifyClients.Add(guild, spotifyBot);
    }

    public void AddSpotifyClient(SocketGuild guild)
        => AddSpotifyClientCmd(guild);

    public static void StartSpotifyForGuildCommand(ulong guildId)
    {
        var guildPair = _spotifyClients.FirstOrDefault(pair => pair.Key.Id == guildId);
        if (guildPair.Key == null)
            return;
        
        var threadSpotify = new Thread(guildPair.Value.RunBot().GetAwaiter().GetResult);
        threadSpotify.Start();
    }

    public void StartSpotifyForGuild(ulong guildId)
        => StartSpotifyForGuildCommand(guildId);

    public static bool IsSpotifyActive(ulong guildId)
    {
        var guildPair = _spotifyClients.FirstOrDefault(pair => pair.Key.Id == guildId);
        if (guildPair.Key == null)
            return false;

        var spotify = guildPair.Value;
        return spotify.IsActive();
    }

}