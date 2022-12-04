﻿using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;
using static System.DateTime;

namespace PhipseyyBot.Discord.Services;

public class CommandHandler
{
    private readonly InteractionService _commands;
    private readonly DiscordSocketClient _discord;
    private readonly IServiceProvider _services;

    public CommandHandler(InteractionService commands, DiscordSocketClient discord,
        IServiceProvider services)
    {
        _commands = commands;
        _discord = discord;
        _services = services;
    }

    public async Task Initialize()
    {
        await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
        _discord.InteractionCreated += InteractionCreated;
        _discord.ButtonExecuted += ButtonExecuted;
        _discord.Ready += Ready;
        _commands.SlashCommandExecuted += CommandsOnSlashCommandExecuted;
        _commands.AutocompleteHandlerExecuted += (_, _, _) => Task.CompletedTask;
    }

    private static void LogCommandHandler(string message)
        =>  Log.Warning($"[CommandHandler] {Now:HH:mm:ss} {message}");
    
    private async Task CommandsOnSlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            switch (arg3.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    await arg2.Interaction.RespondAsync(embed: GetErrorEmbed("Unmet Precondition: \n" + arg3.ErrorReason), ephemeral: true);
                    LogCommandHandler($"Command execution failed: Unmet Precondition: {arg3.ErrorReason}");
                    break;
                case InteractionCommandError.BadArgs:
                    await arg2.Interaction.RespondAsync(embed: GetErrorEmbed("Invalid number or arguments"), ephemeral: true);
                    LogCommandHandler("Command execution failed: Invalid number or arguments");
                    break;
                case InteractionCommandError.Exception:
                    await arg2.Interaction.RespondAsync(embed: GetErrorEmbed("Command exception: \n" + arg3.ErrorReason), ephemeral: true);
                    LogCommandHandler($"Command execution failed: {arg3.ErrorReason}");
                    break;
                case InteractionCommandError.Unsuccessful:
                    await arg2.Interaction.RespondAsync(embed: GetErrorEmbed("Command could not be executed"), ephemeral: true);
                    LogCommandHandler("Command could not be executed");
                    break;
                case InteractionCommandError.UnknownCommand:
                    break;
                case InteractionCommandError.ConvertFailed:
                    break;
                case InteractionCommandError.ParseFailed:
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private async Task InteractionCreated(SocketInteraction arg)
    {
        var ctx = new SocketInteractionContext(_discord, arg); 
        await _commands.ExecuteCommandAsync(ctx, _services);
    }
    
    private async Task ButtonExecuted(SocketMessageComponent arg)
    {
        var ctx = new SocketInteractionContext<SocketMessageComponent>(_discord, arg);
        await _commands.ExecuteCommandAsync(ctx, _services);
    }
    
    private async Task Ready()
    {
        await _commands.RegisterCommandsGloballyAsync();
        _discord.Ready -= Ready;
    }

    private static Embed GetErrorEmbed(string errorMessage)
    {
        var embed = new EmbedBuilder()
            .WithTitle("ERROR")
            .WithDescription(errorMessage)
            .WithTimestamp(Now)
            .WithColor(Color.DarkRed)
            .WithFooter("PhipseyyBot - Error");

        return embed.Build();
    }


}