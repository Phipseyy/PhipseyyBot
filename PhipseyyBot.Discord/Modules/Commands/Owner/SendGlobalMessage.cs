#nullable disable
using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using PhipseyyBot.Common.Db.Extensions;
using PhipseyyBot.Common.Services;
using Serilog;

namespace PhipseyyBot.Discord.Modules.Commands.Owner;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]

[RequireOwner]

[Group("global", "[Owner] Sends global messages")]
public class SendGlobalMessage : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("message", "[Owner] Sends message to all servers (for e.g. shout-outs, announcements, etc.) with an attatchment")]
    public async Task SendGlobalMessageCommand(string title,string message, IAttachment attachment = null)
    {
        var dbContext = DbService.GetDbContext();

        var embed = new EmbedBuilder()
            .WithAuthor(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
            .WithTitle(title)
            .WithDescription(message);

        if (attachment != null && attachment.ContentType.Contains("image"))
            embed.WithImageUrl(attachment.Url);

        foreach (var guild in Context.Client.Guilds)
        {
            try
            {
                var channel = dbContext.GetLogChannel(guild);
                await channel.SendMessageAsync(text: "@everyone", embed: embed.Build());
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"[DiscordCommand] ERROR: {ex.Message}");
                await RespondAsync(text: $"ERROR: {ex.Message}", ephemeral: true);
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
                var channel = dbContext.GetLogChannel(guild);
                await channel.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"[DiscordCommand] ERROR: {ex.Message}");
                await RespondAsync(text: $"ERROR: {ex.Message}", ephemeral: true);
            }
        }
        await RespondAsync(text: "Done", ephemeral: true);
        await DeleteOriginalResponseAsync();
    }
}