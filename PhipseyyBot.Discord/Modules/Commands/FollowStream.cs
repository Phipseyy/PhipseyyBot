﻿using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using PhipseyyBot.Common.Db.Extensions;
using PhipseyyBot.Common.Services;
using PhipseyyBot.Discord.Services.PubSub;

namespace PhipseyyBot.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[RequireUserPermission(GuildPermission.Administrator)]
public class FollowStream : InteractionModuleBase<SocketInteractionContext>
{
    [EnabledInDm(false)]
    [SlashCommand("follow-stream", "Activates notifications for a certain Twitch Stream")]
    public async Task FollowStreamCommand(string twitchName)
    {
        var dbContext = DbService.GetDbContext();
        dbContext.FollowStream(Context.Guild.Id, twitchName);
        PubSubService.FollowStream(twitchName);
        await RespondAsync($"This server will now receive notifications when {twitchName} is live");
    }
}