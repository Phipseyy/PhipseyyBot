using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Phipseyy.Common.Services;

namespace Phipseyy.Discord.Modules;

[Name("EmbedTest")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class EmbedTest : InteractionModuleBase<SocketInteractionContext>
{

    [SlashCommand("embed-test", "Debug Embed")]
    public async Task EmbedTestCommand()
    {
        var twitchData = new TwitchStreamData("Philted_", "DEBUG TITLE",
            "https://static-cdn.jtvnw.net/jtv_user_pictures/3195cf0d-7e73-495e-a93b-f0dd3dd1c15e-profile_image-300x300.png");
        
        
        
        
        
        var embed = new EmbedBuilder
        {
            Title = twitchData.Title
        };
        embed.WithAuthor(twitchData.Username)
            .WithFooter(footer => footer.Text = $"PhipseyyBot")
            .WithColor(Color.Red)
            .WithImageUrl(twitchData.UrlToPreview)
            .WithThumbnailUrl(twitchData.UrlToProfilePicture)
            .WithUrl($"https://twitch.tv/{twitchData.Username}")
            .WithCurrentTimestamp();

        //Your embed needs to be built before it is able to be sent
        await ReplyAsync(embed: embed.Build());
        
    }

}