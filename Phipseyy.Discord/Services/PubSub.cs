#nullable disable
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

public class PubSub
{
    private readonly PhipseyyDbContext _dbContext;
    private static ulong _guildId;
    private readonly IBotCredentials _creds;

    private static DiscordBot _discordBot;
    private readonly SpotifyBot _spotifyManager;
    private static TwitchPubSub _pubSub;

    // Spotify Song Request - RewardID
    private const string SpotifySr = "8b993ff7-a3bd-4d0c-89d4-261e5cbad132";

    public PubSub(DiscordBot discordBot, SpotifyBot spotifyManager, ulong guildId, IServiceProvider serviceProvider)
    {
        _dbContext = serviceProvider.GetRequiredService<PhipseyyDbContext>();
        _discordBot = discordBot;
        _spotifyManager = spotifyManager;
        _guildId = guildId;
        _pubSub = new TwitchPubSub();
        _creds = new BotCredsProvider().GetCreds();
    }

    public async Task InitializePubSub()
    {
        var spotifyThread = new Thread(_spotifyManager.RunBot().GetAwaiter().GetResult);
        spotifyThread.Start();

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
        if (e.RewardRedeemed.Redemption.Reward.Id.Equals(SpotifySr))
        {
            LogTwitchPubSub($"Song Request: {e.RewardRedeemed.Redemption.UserInput}");
            _spotifyManager.AddSongToQueue(e.RewardRedeemed.Redemption.UserInput);
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
            _discordBot.SendTextMessage($"OnStreamUp: {exception.Message}", _guildId);
        }
    }

    private void PubSub_OnServiceConnected(object sender, EventArgs e)
    {
        LogTwitchPubSub("---PubSub Connected!---");
        _discordBot.SendTextMessage("PubSub Service online!", _guildId);

        var streams = _dbContext.GetListOfFollowedStreams(_guildId);

        foreach (var config in streams)
            _pubSub.ListenToVideoPlayback(config.ChannelId);

        var mainStream = _dbContext.GetMainStreamOfGuild(_guildId);
        _pubSub.ListenToChannelPoints(mainStream!.ChannelId);


        _pubSub.ListenToVideoPlayback(TwitchConverter.GetTwitchIdFromName(_creds.TwitchUsername));
        _pubSub.ListenToChannelPoints(TwitchConverter.GetTwitchIdFromName(_creds.TwitchUsername));
        _pubSub.SendTopics(_creds.TwitchAccessToken);
    }
    
}