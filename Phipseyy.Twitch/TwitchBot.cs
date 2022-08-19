﻿using Phipseyy.Common;
using Phipseyy.Discord;
using Phipseyy.Spotify;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using static System.DateTime;
using Serilog;

namespace Phipseyy.Twitch;

public class TwitchBot
{
    private readonly IBotCredentials _creds;

    private readonly DiscordBot _discordBot;
    private readonly SpotifyBot _spotifyManager;
    private static TwitchClient _client = null!;
    private static TwitchPubSub _pubSub = null!;

    // Spotify Song Request - RewardID
    private const string SpotifySr = "8b993ff7-a3bd-4d0c-89d4-261e5cbad132";

    public TwitchBot(DiscordBot discordBot, SpotifyBot spotifyManager, IBotCredentials credentials)
    {
        _creds = credentials;
        _discordBot = discordBot;
        _spotifyManager = spotifyManager;

        //initialize PubSub
        _pubSub = new TwitchPubSub();

        //initialize Client
        var twitchCredentials = new ConnectionCredentials(_creds.TwitchUsername, _creds.TwitchAccessToken);
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };
        var customClient = new WebSocketClient(clientOptions);
        _client = new TwitchClient(customClient);
        _client.Initialize(twitchCredentials, _creds.TwitchUsername );
    }

    public async Task RunBot()
    {
        // Start PubSub
        _pubSub.OnPubSubServiceConnected += PubSub_OnServiceConnected;
        _pubSub.OnPubSubServiceClosed += PubSubOn_OnPubSubServiceClosed;
        _pubSub.OnPubSubServiceError += PubSubOn_OnPubSubServiceError;
        _pubSub.OnStreamUp += PubSub_OnStreamUp;
        _pubSub.OnChannelPointsRewardRedeemed += PubSub_OnChannelPointsRewardRedeemed;
        _pubSub.OnLog += PubSub_OnLog;
        _pubSub.Connect();

        //Start Client
        _client.OnLog += Client_OnLog;
        _client.OnJoinedChannel += Client_OnJoinedChannel;
        _client.OnMessageReceived += Client_OnMessageReceived;
        _client.Connect();

        await Task.Delay(-1);
    }


    /* --- Help Functions --- */
    private static void LogTwitchClient(string message)
        => Log.Information($"[TwitchClient] {Now:HH:mm:ss} {message}");
            

    private static void LogTwitchPubSub(string message)
        => Log.Information($"[TwitchPubSub] {Now:HH:mm:ss} {message}");

    /* --- PubSub Events --- */

    //private void PubSub_OnRewardRedeemed(object? sender, OnRewardRedeemedArgs e)
    //{
    //    if (e.RewardId.Equals(spotifySR))
    //    {
    //        LogTwitch($"Song Request: {e.Status}");
    //        if (e.Status == "ACTION_TAKEN")
    //        {
    //            spotifyManager.AddSongToQueue(e.Message);
    //            LogTwitch(spotifyManager.GetCurrentSong());
    //        }
    //    }
    //}

    private static void PubSub_OnLog(object? sender, TwitchLib.PubSub.Events.OnLogArgs e)
        => LogTwitchPubSub(e.Data);

    private void PubSubOn_OnPubSubServiceClosed(object? sender, EventArgs e)
    {
        LogTwitchPubSub("OnPubSubServiceClosed Triggered");
        LogTwitchPubSub($"{e}");
        _pubSub.Connect();
    }

    private static void PubSubOn_OnPubSubServiceError(object? sender, OnPubSubServiceErrorArgs e)
    {
        LogTwitchPubSub("OnPubSubServiceError Triggered");
        LogTwitchPubSub($"{e}");
        LogTwitchPubSub(e.Exception.Message);
        try
        {
            _pubSub.Disconnect();
            _pubSub.Connect();
        }
        catch (Exception exception)
        {
            LogTwitchPubSub($"ERROR - {exception}");
            throw;
        }
    }

    private void PubSub_OnChannelPointsRewardRedeemed(object? sender, OnChannelPointsRewardRedeemedArgs e)
    {
        if (e.RewardRedeemed.Redemption.Reward.Id.Equals(SpotifySr))
        {
            LogTwitchPubSub($"Song Request: {e.RewardRedeemed.Redemption.UserInput}");
            _spotifyManager.AddSongToQueue(e.RewardRedeemed.Redemption.UserInput);
            LogTwitchPubSub(_spotifyManager.GetCurrentSong());
        }
    }

    private void PubSub_OnStreamUp(object? sender, OnStreamUpArgs e)
    {
        LogTwitchPubSub("Stream is up now!");
        _discordBot.SendTextMessage($"{_creds.TwitchUsername} is now LIVE @everyone! \nhttps://www.twitch.tv/{_creds.TwitchUsername}");
    }

    private void PubSub_OnServiceConnected(object? sender, EventArgs e)
    {
        LogTwitchPubSub("---PubSub Connected!---");
        _pubSub.ListenToVideoPlayback(_creds.TwitchId);
        _pubSub.ListenToChannelPoints(_creds.TwitchId);
        _pubSub.SendTopics(_creds.TwitchAccessToken);
    }

    /* --- Client Events --- */
    private static void Client_OnLog(object? sender, TwitchLib.Client.Events.OnLogArgs e)
        => LogTwitchClient(e.Data);

    private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        LogTwitchClient("---Bot joined!---");
        _discordBot.SendTextMessage("Connected!");
    }

    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        //Twitch Commands
        if (e.ChatMessage.Message.Equals("!song"))
            _client.SendMessage(_client.JoinedChannels[0], _spotifyManager.GetCurrentSong());
    }
}