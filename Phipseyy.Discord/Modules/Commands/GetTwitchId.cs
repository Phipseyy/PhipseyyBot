using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Phipseyy.Common.Modules;
using Phipseyy.Common.Services;

namespace Phipseyy.Discord.Modules.Commands;

[Name("GetTwitchId")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[global::Discord.Interactions.RequireUserPermission(GuildPermission.Administrator)]
public class GetTwitchId : InteractionModuleBase<SocketInteractionContext>
{

    [SlashCommand("get_twitchid", "Tries to grab the TwitchID from an Twitch Account using the name")]
    public async Task GetTwitchIdFromNameCommand(string name)
    {
        try
        {
            await RespondAsync(TwitchConverter.GetTwitchIdFromName(name));
        }
        catch (Exception e)
        {
            await RespondAsync(e.Message);
        }

    }

}