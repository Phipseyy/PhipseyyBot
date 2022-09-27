#nullable disable
using Phipseyy.Common;
using Phipseyy.Common.Services;
using Phipseyy.Discord;
using Phipseyy.Spotify;
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
    private static IBotCredentials _creds;

    private static DiscordBot _discordBot;
    private readonly SpotifyBot _spotifyManager;
    private static TwitchClient _client;
    

    public TwitchBot(DiscordBot discordBot, SpotifyBot spotifyManager)
    {
        var credsProvider = new BotCredsProvider();
        _creds = credsProvider.GetCreds();

        _discordBot = discordBot;
        _spotifyManager = spotifyManager;
        

        //initialize Client
        var twitchCredentials = new ConnectionCredentials(_creds.TwitchUsername, _creds.TwitchAccessToken);
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };
        var customClient = new WebSocketClient(clientOptions);
        _client = new TwitchClient(customClient);
        _client.Initialize(twitchCredentials, _creds.TwitchUsername);
    }

    public async Task RunBot()
    {
        _client.OnLog += Client_OnLog;
        _client.OnJoinedChannel += Client_OnJoinedChannel;
        _client.OnMessageReceived += Client_OnMessageReceived;
        _client.Connect();


        await Task.Delay(-1);
    }

    /* --- Help Functions --- */
    private static void LogTwitchClient(string message)
        => Log.Information($"[TwitchClient] {Now:HH:mm:ss} {message}");
    

    public static Task RestartServices()
    {
        _client.Disconnect();
        _client.Connect();

        return Task.CompletedTask;
    }


    /* --- Client Events --- */
    private static void Client_OnLog(object sender, OnLogArgs e)
        => LogTwitchClient(e.Data);

    private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
    {
        LogTwitchClient("---Bot joined!---");
        _discordBot.SendTextMessage("Connected!");
    }

    private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        //Twitch Commands
        if (e.ChatMessage.Message.Equals("!song"))
            _client.SendMessage(_client.JoinedChannels[0], _spotifyManager.GetCurrentSong());
    }
}