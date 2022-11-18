using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using Phipseyy.Common.Services;

namespace Phipseyy.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[RequireOwner]
public class EditStatus : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("set-status", "[Owner] Changes the status of the Discord Bot")]
    public async Task EditStatusCommand(string status)
    {
        var creds = new BotCredsProvider().GetCreds();
        creds.DiscordStatus = status;
        var credsProvider = new BotCredsProvider();
        
        credsProvider.OverrideSettings(creds);
        await RespondAsync($"Status changed to {status}");
    }
}