using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Phipseyy.Common.Db.Extensions;
using Phipseyy.Common.Services;

namespace Phipseyy.Discord.Modules.Commands;


[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireUserPermission(GuildPermission.Administrator)]
public class SetStream : InteractionModuleBase<SocketInteractionContext>
{
    [EnabledInDm(false)]
    [SlashCommand("set-stream", "Sets the main Stream for this Server")]
    public async Task SetStreamCommand(string twitchName)
    {
        var dbContext = DbService.GetDbContext();
        dbContext.SetMainStream(Context.Guild.Id, twitchName);
        await RespondAsync($"Main stream has been set to {twitchName}");
    }
}