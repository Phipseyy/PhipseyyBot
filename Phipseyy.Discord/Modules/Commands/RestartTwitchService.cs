using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Phipseyy.Discord.Services;

namespace Phipseyy.Discord.Modules.Commands;

[Name("RestartTwitchService")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[global::Discord.Interactions.RequireUserPermission(GuildPermission.Administrator)]
public class RestartTwitchService : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("restart-twitchservice", "Try to restart the currently running instance of the Twitch Service")]
    public async Task RestartTwitch()
    {
        await RespondAsync("Restarting service");
        await PubSub.RestartService();
        await ReplyAsync("Service restarted!");
    }

}