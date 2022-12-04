using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using PhipseyyBot.Common.Db.Extensions;
using PhipseyyBot.Common.Services;

namespace PhipseyyBot.Discord.Modules.Commands;


[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireUserPermission(GuildPermission.Administrator)]
public class SetStream : InteractionModuleBase<SocketInteractionContext>
{
    [EnabledInDm(false)]
    [SlashCommand("set-streamer", "Sets the main Stream for this Server")]
    public async Task SetStreamCommand(string twitchName)
    {
        var dbContext = DbService.GetDbContext();
        if (dbContext.SetMainStream(Context.Guild.Id, twitchName))
            await RespondAsync($"Main stream has been set to {twitchName}");
        else
            await RespondAsync($"{twitchName} is already the Main Stream from another Server");
    }
}