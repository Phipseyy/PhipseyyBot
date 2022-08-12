using System;
using System.Threading;
using System.Threading.Tasks;
using Phipsey.Twitch;
using Phipseyy.Common.Services;
using Phipseyy.Discord;
using Phipseyy.Spotify;

namespace PhipseyyBot;

public static class Bot
{
    public static Task StartupBot()
    {
        var settings = new SettingsHandler(AppContext.BaseDirectory+ "/config.json");
        
        var discordBot = new DiscordBot(settings);
        var spotifyBot = new SpotifyBot(settings, "spotifyCred.json");
        var twitchBot = new TwitchBot(discordBot, spotifyBot, settings);

        var dcBotThread = new Thread(discordBot.RunBot().GetAwaiter().GetResult);
        dcBotThread.Start();

        var spotifyThread = new Thread(spotifyBot.RunBot().GetAwaiter().GetResult);
        spotifyThread.Start();

        var twitchBotThread = new Thread(twitchBot.RunBot().GetAwaiter().GetResult);
        twitchBotThread.Start();

        return Task.Delay(-1);
    }
}