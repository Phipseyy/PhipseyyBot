#nullable disable
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PhipseyyBot.Common;
using PhipseyyBot.Common.Db;
using PhipseyyBot.Common.Db.Extensions;
using PhipseyyBot.Common.Services;
using PhipseyyBot.Discord.Services;
using PhipseyyBot.Discord.Services.PubSub;
using Serilog;
using RunMode = Discord.Commands.RunMode;

namespace PhipseyyBot.Discord;

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
            DefaultRunMode = RunMode.Async
        });

        DcClient = new DiscordSocketClient(new DiscordSocketConfig
        {
            DefaultRetryMode = RetryMode.AlwaysRetry,
            LogLevel = LogSeverity.Info,
            ConnectionTimeout = int.MaxValue,
            AlwaysDownloadUsers = true
        });

        var svcs = new ServiceCollection()
            .AddSingleton(_creds)
            .AddSingleton(DcClient)
            .AddSingleton(commands)
            .AddSingleton<PubSubService>()
            .AddSingleton<InteractionService>()
            .AddSingleton<CommandHandler>()
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
        DcClient.SelectMenuExecuted += BotClientOnSelectMenuExecuted;

        _credsProvider.ConfigfileEdited += CredsProviderOnConfigfileEdited;

        await DcClient.LoginAsync(TokenType.Bot, _creds.DiscordToken);
        await DcClient.StartAsync();

        var commandHandler = Services.GetRequiredService<CommandHandler>();
        await commandHandler.Initialize();

        var pubSubService = Services.GetRequiredService<PubSubService>();
        await pubSubService.InitializePubSub();

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

    private async Task BotClientOnJoinedGuild(SocketGuild arg)
    {
        LogDiscord($"Joined server: {arg.Name} [{arg.Id}]");
        await InitializeGuild(arg);
    }

    private async Task ClientReady()
    {
        foreach (var guild in DcClient.Guilds)
            await InitializeGuild(guild);
        await DcClient.SetGameAsync(_creds.DiscordStatus, $"https://www.twitch.tv/{_creds.TwitchUsername}",
            ActivityType.Streaming);
        LogDiscord("---Bot is Ready!---");
    }

    private async Task BotClientOnDisconnected(Exception arg)
    {
        try
        {
            LogDiscord("Client disconnected! Trying to reconnect in a sec.");
            await Task.Delay(2000);
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

    private async Task BotClientOnSelectMenuExecuted(SocketMessageComponent arg)
    {
        if (arg.GuildId != null && arg.Data.CustomId == "rew-menu")
        {
            _dbContext.SetSongRequestForStream(arg.GuildId.Value, arg.Data.Values.ElementAt(0));
            await arg.Message.DeleteAsync();
            await arg.RespondAsync(text: "Song Request has been set!", ephemeral: true);
        }
    }


    /* ---Methods--- */
    public async Task SendStreamNotification(TwitchStreamData streamData)
    {
        foreach (var guild in DcClient.Guilds)
        {
            var currentConfig = _dbContext.TwitchConfigs.FirstOrDefault(config =>
                config.GuildId == guild.Id && config.ChannelId == streamData.ChannelId);
            var isMainStream = currentConfig != null && currentConfig.ChannelId == streamData.ChannelId && currentConfig.MainStream;
            
            var guildConfig = _dbContext.GetGuildConfig(guild.Id);
            if (guildConfig == null) continue;
            if (currentConfig == null) continue;

            try
            {
                var liveChannel = _dbContext.GetLiveChannel(guild); 
                var partnerChannel = _dbContext.GetPartnerChannel(guild);

                if (isMainStream && liveChannel != null)
                {
                    await Task.Run(()
                        => liveChannel.SendMessageAsync(
                            text:
                            TwitchStringHelper.ParseTwitchNotification(guildConfig.MainStreamNotification, streamData),
                            embed: streamData.GetDiscordEmbed()));
                }
                else if (partnerChannel != null)
                {
                    await Task.Run(()
                        => partnerChannel.SendMessageAsync(
                            text:
                            TwitchStringHelper.ParseTwitchNotification(guildConfig.PartnerStreamNotification, streamData),
                            embed: streamData.GetDiscordEmbed()));
                }
            }
            catch (Exception ex)
            {
                LogDiscord($"ERROR: {ex.Message}");
            }
        }
    }

    public async void SendGlobalLogMessage(string message)
    {
        foreach (var guild in DcClient.Guilds)
        {
            try
            {
                var channel = _dbContext.GetLogChannel(guild);
                await channel.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                LogDiscord(ex.Message);
            }
        }
    }

    private async Task InitializeGuild(SocketGuild guild)
    {
        var currentConfig = _dbContext.GetGuildConfig(guild.Id);
        if (currentConfig == null)
            await SetupGuild(guild);
        else
        {
            await SetupService.VerifyChannels(_dbContext, guild);
            await SetupService.VerifyMessages(_dbContext, guild);
        }

        var twitchConfig = _dbContext.GetMainStreamOfGuild(guild.Id);
        var spotifyConfig = _dbContext.GetSpotifyConfigFromGuild(guild.Id);
        if (spotifyConfig == null || twitchConfig == null)
            return;

        PubSubService.AddSpotifyClient(guild);
        PubSubService.StartSpotifyForGuild(guild.Id);

        LogDiscord($"Done starting Services for {guild.Name}");
    }

    /// <summary>
    /// Sets up the Guild for the first-time usage
    /// </summary>
    /// <param name="socketGuild"></param>
    private async Task SetupGuild(SocketGuild socketGuild)
    {
        await SetupService.InitializeChannels(_dbContext, socketGuild);
    }
}