#nullable disable
using Phipseyy.Twitch;
using Phipseyy.Discord;
using Phipseyy.Spotify;

namespace PhipseyyBot;

public static class Bot
{

    public static Task StartupBot()
    {
        var spotifyBot = new SpotifyBot();
        var discordBot = new DiscordBot(spotifyBot);
        var twitchBot = new TwitchBot(discordBot, spotifyBot);

        var dcBotThread = new Thread(discordBot.RunBot().GetAwaiter().GetResult);
        dcBotThread.Start();

        var spotifyThread = new Thread(SpotifyBot.RunBot().GetAwaiter().GetResult);
        spotifyThread.Start();

        var twitchBotThread = new Thread(twitchBot.RunBot().GetAwaiter().GetResult);
        twitchBotThread.Start();

        return Task.Delay(-1);
        
        // dotnet publish PhipseyyBot -r linux-arm64 -p:PublishSingleFile=true --self-contained 

        
    }
}