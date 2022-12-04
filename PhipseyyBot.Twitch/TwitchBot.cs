#nullable disable
using PhipseyyBot.Common;
using PhipseyyBot.Common.Services;
using PhipseyyBot.Discord;
using PhipseyyBot.Spotify;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using static System.DateTime;
using Serilog;

namespace PhipseyyBot.Twitch;

public class TwitchBot
{
    private static IBotCredentials _creds;

    private static DiscordBot _discordBot;
    private static TwitchClient _client;
    

    public TwitchBot(DiscordBot discordBot)
    {
        var credsProvider = new BotCredsProvider();
        _creds = credsProvider.GetCreds();

        _discordBot = discordBot;
        
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
    { 
        if(!message.Contains("PING") || !message.Contains("PONG"))
            Log.Information($"[TwitchClient] {Now:HH:mm:ss} {message}");
    }

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
        LogTwitchClient($"---Bot joined Twitch Channel {e.Channel}!---");
    }

    private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        //TODO REWORK
        //Twitch Commands
        //if (e.ChatMessage.Message.Equals("!song"))
            
            //_client.SendMessage(_client.JoinedChannels[0], SpotifyBot.GetCurrentSong());
    }
}