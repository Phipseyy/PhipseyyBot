#nullable disable
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Phipseyy.Common;
using Phipseyy.Common.Db;
using Phipseyy.Common.Db.Extensions;
using Phipseyy.Common.Services;
using Phipseyy.Discord.Services;
using RunMode = Discord.Commands.RunMode;

namespace Phipseyy.Discord;

public class DiscordBot
{
    private readonly IBotCredentials _creds;
    private readonly BotCredsProvider _credsProvider;
    
    private readonly PhipseyyDbContext _dbContext;

    private DiscordSocketClient DcClient { get; }
    private IServiceProvider Services { get; }

    public DiscordBot()
    {
        _credsProvider = new BotCredsProvider();
        _creds = _credsProvider.GetCreds();
        _dbContext = DbService.GetDbContext();

        var commands = new CommandService(new CommandServiceConfig
        {
            CaseSensitiveCommands = false,
            DefaultRunMode = RunMode.Sync
        });

        DcClient = new DiscordSocketClient(new DiscordSocketConfig
        {
            DefaultRetryMode = RetryMode.AlwaysRetry,
            LogLevel = LogSeverity.Info,
            ConnectionTimeout = int.MaxValue,
            AlwaysDownloadUsers = true
        });

        var svcs = new ServiceCollection()
            .AddSingleton(_creds) //just in case we need it at some point
            .AddSingleton(DcClient)
            .AddSingleton(commands)
            .AddSingleton<CommandHandler>()
            .AddSingleton<PubSubService>()
            .AddSingleton<InteractionService>()
            .AddDbContext<PhipseyyDbContext>()
            .AddSingleton(this);

        Services = svcs.BuildServiceProvider();
    }

    public async Task RunBot()
    {
        //Events
        DcClient.Log += Logging;
        DcClient.Ready += ClientReady;
        DcClient.Connected += BotClientOnConnected;
        DcClient.Disconnected += BotClientOnDisconnected;
        DcClient.JoinedGuild += BotClientOnJoinedGuild;

        _credsProvider.ConfigfileEdited += CredsProviderOnConfigfileEdited;

        await DcClient.LoginAsync(TokenType.Bot, _creds.DiscordToken);
        await DcClient.StartAsync();

        var commandHandler = Services.GetRequiredService<CommandHandler>();
        await commandHandler.Initialize();

        await Task.Delay(-1);
    }

    /* ---Helpers--- */

    /// <summary>
    /// Fancy Console Output
    /// </summary>
    /// <param name="message"></param>
    private static void LogDiscord(string message)
        => Log.Information($"[Discord] {message}");

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

    private Task BotClientOnJoinedGuild(SocketGuild arg)
    {
        LogDiscord($"Joined server: {arg.Name} [{arg.Id}]");
        InitializeGuild(arg);
        return Task.CompletedTask;
    }

    private async Task ClientReady()
    {
        foreach (var guild in DcClient.Guilds)
            InitializeGuild(guild);
        await DcClient.SetGameAsync(_creds.DiscordStatus, $"https://www.twitch.tv/{_creds.TwitchUsername}",
            ActivityType.Streaming);
        LogDiscord("---Bot is Ready!---");
        SendGlobalLogMessage($" {DateTime.Now} - All Services online! GLHF");
    }

    private async Task BotClientOnDisconnected(Exception arg)
    {
        try
        {
            LogDiscord("Client disconnected! Trying to reconnect in 10sec.");
            await Task.Delay(10000);
            await DcClient.StartAsync();
            await Task.Delay(1000);
            if (DcClient.ConnectionState != ConnectionState.Connected)
                await BotClientOnDisconnected(arg);
        }
        catch (Exception e)
        {
            LogDiscord(e.Message);
        }
    }

    private static Task BotClientOnConnected()
    {
        LogDiscord("Client connected!");
        return Task.CompletedTask;
    }

    private async void CredsProviderOnConfigfileEdited(object sender, EventArgs e)
    {
        await DcClient.SetGameAsync(_creds.DiscordStatus, $"https://www.twitch.tv/{_creds.TwitchUsername}",
            ActivityType.Streaming);
    }

    /* ---Methods--- */

    public void SendTextMessage(string message, ulong guildId)
    {
        try
        {
            var dcConfig = _dbContext.GetGuildConfig(guildId);
            var channel = DcClient.GetChannel(dcConfig.LogChannel) as IMessageChannel;
            channel!.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            LogDiscord($"ERROR: {ex.Message}");
        }
    }

    public async Task SendStreamNotification(TwitchStreamData streamData)
    {
        foreach (var guild in DcClient.Guilds)
        {
            var currentConfig = _dbContext.TwitchConfigs.FirstOrDefault(config =>
                config.GuildId == guild.Id && config.ChannelId == streamData.ChannelId);
            if (currentConfig == null) continue;

            try
            {
                var channelId = _dbContext.GuildConfigs.FirstOrDefault(config => config.GuildId == guild.Id)!.GuildId;
                var channel = DcClient.GetChannel(channelId) as IMessageChannel;
                await Task.Run(()
                    => channel?.SendMessageAsync(
                        text:
                        $"Hey @everyone! {streamData.Username} is now live!\nhttps://twitch.tv/{streamData.Username}",
                        embed: streamData.GetDiscordEmbed()));
            }
            catch (Exception ex)
            {
                LogDiscord($"ERROR: {ex.Message}");
            }
        }
    }

    private async void SendGlobalLogMessage(string message)
    {
        foreach (var guild in DcClient.Guilds)
        {
            try
            {
                var channel = guild.TextChannels.FirstOrDefault(x => x!.Name == "log", null);
                await channel!.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                LogDiscord(ex.Message);
            }
        }
    }

    private void InitializeGuild(SocketGuild guild)
    {
        var currentConfig = _dbContext.GetGuildConfig(guild.Id);
        if (currentConfig == null)
            SetupGuild(guild);

        var twitchConfig = _dbContext.GetMainStreamOfGuild(guild.Id);
        var spotifyConfig = _dbContext.GetSpotifyConfigFromGuild(guild.Id);
        if (spotifyConfig == null || twitchConfig == null)
            return;

        var svc = Services.GetRequiredService<PubSubService>();
        svc.AddGuild(guild);
        svc.StartServiceForGuild(guild.Id);
        
        LogDiscord($"Done starting Services for {guild.Name}");
    }

    /// <summary>
    /// Sets up the Guild for the first-time usage
    /// </summary>
    /// <param name="socketGuild"></param>
    private void SetupGuild(SocketGuild socketGuild)
    {
        SetupService.InitializeChannels(_dbContext, socketGuild);
        
    }
}