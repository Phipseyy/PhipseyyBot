using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Phipseyy.Common.Services;

namespace Phipseyy.Discord.Modules.Commands;

[Name("EmbedTest")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[global::Discord.Interactions.RequireUserPermission(GuildPermission.Administrator)]
public class EmbedTest : InteractionModuleBase<SocketInteractionContext>
{

    [SlashCommand("embed-test", "Debug Embed")]
    public async Task EmbedTestCommand()
    {
        var twitchData = new TwitchStreamData("Philted_", "DEBUG TITLE",
            "https://static-cdn.jtvnw.net/jtv_user_pictures/3195cf0d-7e73-495e-a93b-f0dd3dd1c15e-profile_image-300x300.png",
            "Valorant");


        await RespondAsync("Done");
        await DeleteOriginalResponseAsync();
        await ReplyAsync(text: $"Hey @everyone! {twitchData.Username} is live again!", embed: twitchData.GetDiscordEmbed());
        
    }

}