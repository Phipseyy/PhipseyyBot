using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Phipseyy.Common;
using Phipseyy.Common.Services;
using Phipseyy.Discord.Services;
using RunMode = Discord.Commands.RunMode;


namespace Phipseyy.Discord;

public class DiscordBot
{
    private readonly IBotCredentials _creds;
    private readonly CommandService _commands;

    private DiscordSocketClient BotClient { get; }
    private IServiceProvider Services { get; }

    public DiscordBot()
    {
        _creds = new BotCredsProvider().GetCreds();
        
        _commands = new CommandService(new CommandServiceConfig
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

        Services = InitializeServices();
    }

    public async Task RunBot()
    {
        await BotClient.LoginAsync(TokenType.Bot, _creds.DiscordToken);
        await BotClient.StartAsync();

        //Events
        BotClient.Log += Logging;
        BotClient.Ready += ClientReady;

        var commandHandler = Services.GetRequiredService<CommandHandler>();
        await commandHandler.Initialize();

        LogDiscord(BotClient.LoginState.ToString());

        await Task.Delay(-1);
    }
    
    /* ---Helpers--- */
    
    /// <summary>
    /// Fancy Console Output
    /// </summary>
    /// <param name="message"></param>
    private static void LogDiscord(string message)
        =>  Log.Information("[Discord] {Message}", message);

    private ServiceProvider InitializeServices()
    {
        var svcs = new ServiceCollection()
            .AddSingleton(_creds) //just in case we need it at some point
            .AddSingleton(BotClient)
            .AddSingleton(_commands)
            .AddSingleton<CommandHandler>()
            .AddSingleton<InteractionService>();

        return svcs.BuildServiceProvider();
    }

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

        var applicationCommandProperties = new List<ApplicationCommandProperties>();
        var handler = new SlashCommandBuilder();
        try
        {
            // Testing a reaction role
            handler.WithName("role-handler");
            handler.WithDescription("Sends a Message where people can react to get a certain role");
            handler.AddOption("message", ApplicationCommandOptionType.String, "The message which should be displayed", isRequired: true);
            handler.AddOption("role", ApplicationCommandOptionType.Role, "The role which should be given", isRequired: true);
            handler.AddOption("reaction", ApplicationCommandOptionType.String, "The emoji as an reaction", isRequired: true);

            applicationCommandProperties.Add(handler.Build());

            await BotClient.BulkOverwriteGlobalApplicationCommandsAsync(applicationCommandProperties.ToArray());
        }
        catch (HttpException exception)
        {
            exception.Errors.ToList().ForEach(error => LogDiscord("Client Ready Error: " + error.ToString()));
        }
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
    
    public async void SendStreamNotification(TwitchStreamData streamData)
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