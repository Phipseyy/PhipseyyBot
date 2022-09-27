using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Phipseyy.Common.Services;
using static System.DateTime;

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
        
        var embed = new EmbedBuilder()
            .WithAuthor(twitchData.Username, twitchData.UrlToProfilePicture, $"https://twitch.tv/{twitchData.Username}")
            .WithTitle(twitchData.Title)
            .WithFooter(footer => footer.Text = $"PhipseyyBot")
            .WithColor(Color.Red)
            .WithImageUrl(twitchData.UrlToPreview)
            .WithThumbnailUrl(twitchData.UrlToProfilePicture)
            .WithUrl($"https://twitch.tv/{twitchData.Username}")
            .WithCurrentTimestamp();

        embed.Description = $"Streaming: ``{twitchData.Game}`` \nLive since: ``{Now:HH:mm:ss} CEST``";

        await RespondAsync("Done");
        await DeleteOriginalResponseAsync();
        await ReplyAsync(embed: embed.Build());
        
    }

}