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
    public IBotCredentials Creds { get; set; }
    public IBotCredsProvider CredsProvider { get; set; }
    public PhipseyyDbContext DbContext { get; set; }
    public DiscordSocketClient DcClient { get; set; }
    public IServiceProvider Services { get; set; }


    public DiscordBot()
    {
        CredsProvider = new BotCredsProvider();
        Creds = CredsProvider.GetCreds();
        DbContext = DbService.GetDbContext();

        var commands = new CommandService(new CommandServiceConfig
        {
            CaseSensitiveCommands = false,
            DefaultRunMode = RunMode.Async,
            LogLevel = LogSeverity.Info
        });

        DcClient = new DiscordSocketClient(new DiscordSocketConfig
        {
            DefaultRetryMode = RetryMode.AlwaysRetry,
            LogLevel = LogSeverity.Info,
            ConnectionTimeout = int.MaxValue,
            AlwaysDownloadUsers = true
        });

        var svcs = new ServiceCollection()
            .AddSingleton(Creds)
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

        CredsProvider.ConfigfileEdited += CredsProviderOnConfigfileEdited;

        await DcClient.LoginAsync(TokenType.Bot, Creds.DiscordToken);
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
        await DcClient.SetGameAsync(Creds.DiscordStatus, $"https://www.twitch.tv/{Creds.TwitchUsername}",
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
        await DcClient.SetGameAsync(Creds.DiscordStatus, $"https://www.twitch.tv/{Creds.TwitchUsername}",
            ActivityType.Streaming);
    }

    private async Task BotClientOnSelectMenuExecuted(SocketMessageComponent arg)
    {
        if (arg.GuildId != null && arg.Data.CustomId == "rew-menu")
        {
            var guild = DcClient.Guilds.FirstOrDefault(currentGuild => currentGuild.Id == arg.GuildId.Value);
            await DbContext.SetSongRequestForStreamAsync(guild, arg.Data.Values.ElementAt(0));
            await arg.Message.DeleteAsync();
            await arg.RespondAsync(text: "Song Request has been set!", ephemeral: true);
        }
    }


    /* ---Methods--- */
    public async Task SendStreamNotification(TwitchStreamData streamData)
    {
        foreach (var guild in DcClient.Guilds)
        {
            var currentConfig = DbContext.TwitchConfigs.FirstOrDefault(config =>
                config.GuildId == guild.Id && config.ChannelId == streamData.ChannelId);
            var isMainStream = currentConfig != null && currentConfig.ChannelId == streamData.ChannelId && currentConfig.MainStream;

            var guildConfig = await DbContext.GetGuildConfigAsync(guild);
            
            if (guildConfig == null || currentConfig == null) 
                continue;

            try
            {
                var liveChannel = await DbContext.GetLiveChannelAsync(guild);
                var partnerChannel = await DbContext.GetPartnerChannelAsync(guild);

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
                            TwitchStringHelper.ParseTwitchNotification(guildConfig.PartnerStreamNotification,
                                streamData),
                            embed: streamData.GetDiscordEmbed()));
                }
            }
            catch (Exception ex)
            {
                LogDiscord($"ERROR: {ex.Message}");
            }
        }
    }

    public async Task SendGlobalLogMessage(string message)
    {
        foreach (var guild in DcClient.Guilds)
        {
            try
            {
                var channel = await DbContext.GetLogChannelAsync(guild);
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
        var currentConfig = await DbContext.GetGuildConfigAsync(guild);
        if (currentConfig == null)
            await SetupService.InitializeChannels(DbContext, guild);
        else
        {
            await SetupService.VerifyChannels(DbContext, guild);
            await SetupService.VerifyMessages(DbContext, guild);
        }

        var twitchConfig = await DbContext.GetMainStreamOfGuildAsync(guild);
        var spotifyConfig = await DbContext.GetSpotifyConfigAsync(guild.Id);
        if (spotifyConfig != null && twitchConfig != null)
        {
            PubSubService.AddSpotifyClient(guild);
            PubSubService.StartSpotifyForGuild(guild.Id);
        }

        LogDiscord($"Done starting Services for {guild.Name}");
    }
}