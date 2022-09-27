using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Phipseyy.Common.Services;

namespace Phipseyy.Discord.Modules.Commands;

[Name("edit-status")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[global::Discord.Interactions.RequireUserPermission(GuildPermission.Administrator)]
public class EditStatus : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("edit-status", "Changes the status of the Discord Bot")]
    public async Task Edit(string status)
    {
        var creds = new BotCredsProvider().GetCreds();
        creds.DiscordStatus = status;
        var credsProvider = new BotCredsProvider();
        credsProvider.OverrideSettings(creds);

        await RespondAsync($"Status changed to {status}");
    }
}