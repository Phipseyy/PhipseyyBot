using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Phipseyy.Common.Services;
using Serilog;


namespace Phipseyy.Discord;

public class DiscordBot
{
    private readonly string _discordToken;
    private readonly string _twitchName;
    private readonly string _customStatus;

    private DiscordSocketClient BotClient { get; set; }
    public CommandService Commands { get; set; }
    public IServiceProvider Services { get; set; }

    public DiscordBot(SettingsHandler settings)
    {
        _discordToken = settings.DiscordToken;
        _customStatus = settings.DiscordStatus;
        _twitchName = settings.TwitchUsername;
    }

    public async Task RunBot()
    {
        BotClient = new DiscordSocketClient(new DiscordSocketConfig
        {
            DefaultRetryMode = RetryMode.AlwaysRetry,
            LogLevel = LogSeverity.Info,
            ConnectionTimeout = int.MaxValue
        });
        await BotClient.LoginAsync(TokenType.Bot, _discordToken);
        await BotClient.StartAsync();

        //Events
        BotClient.Log += Logging;
        BotClient.Ready += ClientReady;
        BotClient.SlashCommandExecuted += SlashCommandExecuted;
        BotClient.ReactionAdded += OnReactionAdded;
        LogDiscord(BotClient.LoginState.ToString());

        await Task.Delay(-1);
    }

    private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    {
        //if(arg1.Id.Equals(Settings.lastchannel))
        {
            IUser user = arg3.User.Value;
            await ((IGuildUser)user).AddRoleAsync(960586503011070012);
        }
    }

    /// <summary>
    /// Fancy Console Output
    /// </summary>
    /// <param name="message"></param>
    private static void LogDiscord(string message)
        =>  Log.Information($"[Discord] {message}");

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
        await BotClient.SetGameAsync(_customStatus, $"https://www.twitch.tv/{_twitchName}", ActivityType.Streaming);
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

    private async Task SlashCommandExecuted(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "role-handler":
                await SendMessageWithReaction(command);
                break;
        }
    }

    private async Task SendMessageWithReaction(SocketSlashCommand command)
    {
        var message = await command.Channel.SendMessageAsync(command.Data.Options.ElementAt(0).Value.ToString());
        Emote emote = Emote.Parse(command.Data.Options.ElementAt(2).Value.ToString());
        await message.AddReactionAsync(emote);
    }

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
}