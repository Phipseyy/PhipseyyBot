﻿using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using Discord.WebSocket;
using PhipseyyBot.Discord.Services.PubSub;

namespace PhipseyyBot.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireOwner]
public class IsSpotifyOnline : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping-spotify", "Checks if your spotify bot is running")]
    public async Task IsSpotifyOnlineCommand()
    {
        var active = PubSubService.IsSpotifyActive(Context.Guild.Id);
        if (active)
            await RespondAsync(
                text: "Spotify is currently running!",
                ephemeral: true);
        else
            await RespondAsync(
                text: "Spotify is currently offline!",
                ephemeral: true);
            
    }
}