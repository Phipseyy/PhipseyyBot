using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Discord;
using Discord.Interactions;
using PhipseyyBot.Common.Embeds;

namespace PhipseyyBot.Discord.Modules.Commands;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[RequireUserPermission(GuildPermission.Administrator)]
[Group("help", "Help Commands")]
[EnabledInDm(false)]
public class HelpSettings : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("set-noti-message", "How to set your stream notification message")]
    public async Task SetNotiMessageCommand()
    {
        await RespondAsync(embed: Context.Client.GetMessageInfoEmbed(), ephemeral: true);

    }
    
    [SlashCommand("debug", "Debuggies")]
    public async Task DebugCmd()
    {
        Process currentProcess = Process.GetCurrentProcess();

        var usedMemoryinMb = Math.Round((double)currentProcess.PrivateMemorySize64 / 1000 / 1000, 2);
        
        await RespondAsync(text: usedMemoryinMb.ToString(CultureInfo.InvariantCulture), ephemeral: true);

    }
}