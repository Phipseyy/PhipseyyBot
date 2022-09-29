using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Phipseyy.Common;
using Phipseyy.Common.Services;
using Phipseyy.Discord.Services;
using Phipseyy.Spotify;
using RunMode = Discord.Commands.RunMode;


namespace Phipseyy.Discord;

public class DiscordBot
{
    private readonly IBotCredentials _creds;
    private readonly BotCredsProvider _credsProvider;

    private DiscordSocketClient BotClient { get; }
    private IServiceProvider Services { get; }

    public DiscordBot(SpotifyBot spotifyBot)
    {
        _credsProvider = new BotCredsProvider();
        _creds = _credsProvider.GetCreds();
        
        var commands = new CommandService(new CommandServiceConfig
        {
            CaseSensitiveCommands = false,
            DefaultRunMode = RunMode.Sync
        });
        
        BotClient = new DiscordSocketClient(new DiscordSocketConfig
        {
            DefaultRetryMode = RetryMode.AlwaysRetry,
            LogLevel = LogSeverity.Info,
            ConnectionTimeout = int.MaxValue
        });

        var svcs = new ServiceCollection()
            .AddSingleton(_creds) //just in case we need it at some point
            .AddSingleton(BotClient)
            .AddSingleton(commands)
            .AddSingleton(spotifyBot)
            .AddSingleton<CommandHandler>()
            .AddSingleton<InteractionService>()
            .AddSingleton(this)
            .AddSingleton<PubSub>();
        
        
        Services = svcs.BuildServiceProvider();
    }

    public async Task RunBot()
    {
        //Events
        BotClient.Log += Logging;
        BotClient.Ready += ClientReady;
        BotClient.Connected += BotClientOnConnected;
        BotClient.Disconnected += BotClientOnDisconnected;

        _credsProvider.ConfigfileEdited += CredsProviderOnConfigfileEdited;
        
        await BotClient.LoginAsync(TokenType.Bot, _creds.DiscordToken);
        await BotClient.StartAsync();

        var commandHandler = Services.GetRequiredService<CommandHandler>();
        await commandHandler.Initialize();
        
        var pubSub = Services.GetRequiredService<PubSub>();
        await pubSub.InitializePubSub();

        await Task.Delay(-1);
    }

    /* ---Helpers--- */
    
    /// <summary>
    /// Fancy Console Output
    /// </summary>
    /// <param name="message"></param>
    private static void LogDiscord(string message)
        =>  Log.Information($"[Discord] {message}");

    /* ---Events--- */
    
    /// <summary>
    /// Sends EVERY message to the console
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private static Task Logging(LogMessage message)
    {
        LogDiscord(message.ToString());
        return Task.CompletedTask;
    }
    
    private async Task ClientReady()
    {
        await BotClient.SetGameAsync(_creds.DiscordStatus, $"https://www.twitch.tv/{_creds.TwitchUsername}", ActivityType.Streaming);
        LogDiscord("---Bot is Ready!---");
    }

    private async Task BotClientOnDisconnected(Exception arg)
    {
        try
        {
            LogDiscord("Client disconnected! Trying to reconnect in 10sec.");
            await Task.Delay(10000);
            await BotClient.StartAsync();
            await Task.Delay(1000);
            if (BotClient.ConnectionState != ConnectionState.Connected)
                await BotClientOnDisconnected(arg);
        }
        catch (Exception e)
        {
            LogDiscord(e.Message);
        }
    }
    
    private Task BotClientOnConnected()
    {
        LogDiscord("Client connected!");
        return Task.CompletedTask;
    }
    
    private async void CredsProviderOnConfigfileEdited(object? sender, EventArgs e)
    {
        await BotClient.SetGameAsync(_creds.DiscordStatus, $"https://www.twitch.tv/{_creds.TwitchUsername}", ActivityType.Streaming);
    }
    
    /* ---Methods--- */
    
    public async void SendTextMessage(string message)
    {
        try
        {
            // Watch out for the channelID
            var channel = BotClient.GetChannel(960589669941252106) as IMessageChannel;
            await Task.Run(() => channel?.SendMessageAsync(message));
        }
        catch (Exception ex)
        {
            LogDiscord($"ERROR: {ex.Message}");
        }    
    }
    
    public async Task SendStreamNotification(TwitchStreamData streamData)
    {
        try
        {
            var discordEmbed = streamData.GetDiscordEmbed();
            // Watch out for the channelID
            var channel = BotClient.GetChannel(960589669941252106) as IMessageChannel;
            await Task.Run(() => channel?.SendMessageAsync($"Hey @everyone! {streamData.Username} is now live!", false, discordEmbed.Build()));
        }
        catch (Exception ex)
        {
            LogDiscord($"ERROR: {ex.Message}");
        }    
    }
    
    
}