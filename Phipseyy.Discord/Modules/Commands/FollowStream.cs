﻿using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using Phipseyy.Common.Db.Extensions;
using Phipseyy.Common.Services;

namespace Phipseyy.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[global::Discord.Commands.RequireUserPermission(GuildPermission.Administrator)]
public class FollowStream : InteractionModuleBase<SocketInteractionContext>
{
    [EnabledInDm(false)]
    [SlashCommand("follow", "Activates notifications for a certain Twitch Stream")]
    public async Task FollowStreamCommand(string twitchName)
    {
        var dbContext = DbService.GetDbContext();
        dbContext.FollowStream(Context.Guild.Id, twitchName);
        await RespondAsync($"This server will now receive notifications when {twitchName} is live");
    }
}