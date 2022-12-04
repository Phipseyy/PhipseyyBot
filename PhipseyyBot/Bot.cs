#nullable disable
using PhipseyyBot.Common.Services;
using PhipseyyBot.Twitch;
using PhipseyyBot.Discord;

namespace PhipseyyBot;

public static class Bot
{

    public static async Task StartupBot()
    {
        await DbService.SetupAsync();
        
        var discordBot = new DiscordBot();
        var twitchBot = new TwitchBot(discordBot);
        
        var dcBotThread = new Thread(discordBot.RunBot().GetAwaiter().GetResult);
        dcBotThread.Start();

        var twitchBotThread = new Thread(twitchBot.RunBot().GetAwaiter().GetResult);
        twitchBotThread.Start();

        await Task.Delay(-1);

        // dotnet publish PhipseyyBot -r linux-arm64 -p:PublishSingleFile=true --self-contained 
        
        // dotnet publish PhipseyyBot -r linux-x64 -p:PublishSingleFile=true --self-contained true -c Release -o publish /p:IncludeNativeLibrariesForSelfExtract=true
        
    }
}