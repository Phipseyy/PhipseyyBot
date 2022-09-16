// ReSharper disable UnusedParameter.Local
using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;

namespace Phipseyy.Discord.Services;

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
        _commands.SlashCommandExecuted += (info, context, arg3) => Task.CompletedTask;
        _commands.AutocompleteHandlerExecuted += (handler, context, arg3) => Task.CompletedTask;
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
    
}