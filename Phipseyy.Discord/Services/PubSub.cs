#nullable disable
using Phipseyy.Common;
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
    private static IBotCredentials _creds;

    private static DiscordBot _discordBot;
    private readonly SpotifyBot _spotifyManager;
    private static TwitchPubSub _pubSub;

    // Spotify Song Request - RewardID
    private const string SpotifySr = "8b993ff7-a3bd-4d0c-89d4-261e5cbad132";

    public PubSub(IBotCredentials creds, DiscordBot discordBot, SpotifyBot spotifyManager)
    {
        _creds = creds;
        _discordBot = discordBot;
        _spotifyManager = spotifyManager;
        _pubSub = new TwitchPubSub();
    }

    public async Task InitializePubSub()
    {
        _pubSub.OnPubSubServiceConnected += PubSub_OnServiceConnected;
        _pubSub.OnPubSubServiceClosed += PubSubOn_OnPubSubServiceClosed;
        _pubSub.OnPubSubServiceError += PubSubOn_OnPubSubServiceError;
        _pubSub.OnStreamUp += PubSub_OnStreamUp;
        _pubSub.OnChannelPointsRewardRedeemed += PubSub_OnChannelPointsRewardRedeemed;
        _pubSub.OnLog += PubSub_OnLog;
        _pubSub.Connect();
        
        await Task.Delay(-1);
    }

    /* --- Helpers --- */
    private static void LogTwitchPubSub(string message)
        => Log.Information($"[TwitchPubSub] {Now:HH:mm:ss} {message}");

    public static Task RestartService()
    {
        _pubSub.Disconnect();
        return Task.CompletedTask;
    }
    
    /* --- PubSub Events --- */
    private static void PubSub_OnLog(object sender, OnLogArgs e)
        => LogTwitchPubSub(e.Data);

    private void PubSubOn_OnPubSubServiceClosed(object sender, EventArgs e)
    {
        LogTwitchPubSub("PubSubService got closed! Restarting...");
        _pubSub.Connect();
    }

    private static async void PubSubOn_OnPubSubServiceError(object sender, OnPubSubServiceErrorArgs e)
    {
        try
        {
            LogTwitchPubSub($"OnPubSubServiceError Triggered: {e.Exception.Message}\nRestarting Service in 10sec.");
            _discordBot.SendTextMessage($"OnPubSubServiceError Triggered: {e.Exception.Message}\nRestarting Service in 10sec.");

            _pubSub.Disconnect();
            await Task.Delay(10000);
            _pubSub.Connect();
        }
        catch (Exception exception)
        {
            LogTwitchPubSub($"ERROR - {exception.Message}");
            _discordBot.SendTextMessage($"ERROR - {exception.Message}");
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

            var usersData = api.Helix.Channels.GetChannelInformationAsync(e.ChannelId, _creds.TwitchAccessToken).Result
                .Data[0];
            var user = api.Helix.Search.SearchChannelsAsync(usersData.BroadcasterName, true).Result.Channels[0];


            await _discordBot.SendStreamNotification(new TwitchStreamData(user.DisplayName,
                user.Title,
                user.ThumbnailUrl,
                user.GameName));
        }
        catch (Exception exception)
        {
            LogTwitchPubSub(exception.Message);
            _discordBot.SendTextMessage($"OnStreamUp: {exception.Message}");
        }
    }

    private void PubSub_OnServiceConnected(object sender, EventArgs e)
    {
        LogTwitchPubSub("---PubSub Connected!---");
        _discordBot.SendTextMessage($"PubSub Service online!");
        _pubSub.ListenToVideoPlayback(_creds.TwitchId);
        _pubSub.ListenToChannelPoints(_creds.TwitchId);
        _pubSub.SendTopics(_creds.TwitchAccessToken);
    }
    
}