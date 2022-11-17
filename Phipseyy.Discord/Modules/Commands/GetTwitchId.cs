using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Phipseyy.Common.Modules;

namespace Phipseyy.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireUserPermission(GuildPermission.Administrator)]
public class GetTwitchId : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("get-twitchid", "Tries to grab the TwitchID from an Twitch Account using the name")]
    public async Task GetTwitchIdFromNameCommand(string name)
    {
        await RespondAsync(TwitchConverter.GetTwitchIdFromName(name));
    }

}