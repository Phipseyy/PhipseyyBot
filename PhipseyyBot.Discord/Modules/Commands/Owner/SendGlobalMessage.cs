#nullable disable
using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using PhipseyyBot.Common;
using PhipseyyBot.Common.Db.Extensions;
using PhipseyyBot.Common.Embeds;
using PhipseyyBot.Common.Services;
using Serilog;

namespace PhipseyyBot.Discord.Modules.Commands.Owner;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireOwner]
[Group("global", "[Owner] Sends global messages")]
public class SendGlobalMessage : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("message", "[Owner] Sends message to all servers (for e.g. shout-outs, announcements, etc.) with an attachment")]
    public async Task SendGlobalMessageCommand(string title, string message, IAttachment attachment = null)
    {
        var dbContext = DbService.GetDbContext();

        var embed = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = Context.Client.CurrentUser.Username,
                IconUrl = Context.Client.CurrentUser.GetAvatarUrl()
            },
            Title = title,
            Description = message,
            Color = Const.Main,
            Footer = new EmbedFooterBuilder
            {
                Text = "PhipseyyBot - Announcement"
            },
            Timestamp = DateTime.Now
        };

        if (attachment != null && attachment.ContentType.Contains("image"))
            embed.WithImageUrl(attachment.Url);

        foreach (var guild in Context.Client.Guilds)
        {
            try
            {
                var channel = await dbContext.GetLogChannelAsync(guild);
                await channel.SendMessageAsync(text: "@everyone", embed: embed.Build());
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"[DiscordCommand] ERROR: {ex.Message}");
                await RespondAsync(embed: Context.Client.GetErrorEmbed(ex.GetType().ToString(), ex.Message),
                    ephemeral: true);
            }
        }

        await RespondAsync(text: "Done", ephemeral: true);
        await DeleteOriginalResponseAsync();
    }

    [SlashCommand("chat", "[Owner] Sends message to all servers (for e.g. shout-outs, announcements, etc.)")]
    public async Task SendGlobalChatCommand(string message)
    {
        var dbContext = DbService.GetDbContext();

        foreach (var guild in Context.Client.Guilds)
        {
            try
            {
                var channel = await dbContext.GetLogChannelAsync(guild);
                await channel.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"[DiscordCommand] ERROR: {ex.Message}");
                await RespondAsync(embed: Context.Client.GetErrorEmbed(ex.GetType().ToString(), ex.Message),
                    ephemeral: true);
            }
        }

        await RespondAsync(text: "Done", ephemeral: true);
        await DeleteOriginalResponseAsync();
    }
}