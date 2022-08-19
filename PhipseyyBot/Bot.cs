#nullable disable
using Phipseyy.Common;
using Phipseyy.Twitch;
using Phipseyy.Common.Services;
using Phipseyy.Discord;
using Phipseyy.Spotify;
using Serilog;

namespace PhipseyyBot;

public static class Bot
{
    private static IBotCredsProvider _credsProvider = null!;
    private static IBotCredentials _creds = null!;
    public static Task StartupBot()
    {
        //Testing new Creds
        _credsProvider = new BotCredsProvider();
        _creds = _credsProvider.GetCreds();
        Log.Information(_creds.TwitchUsername);

        var discordBot = new DiscordBot(_creds);
        var spotifyBot = new SpotifyBot(_creds, "spotifyCred.json");
        var twitchBot = new TwitchBot(discordBot, spotifyBot, _creds);

        var dcBotThread = new Thread(discordBot.RunBot().GetAwaiter().GetResult);
        dcBotThread.Start();

        var spotifyThread = new Thread(spotifyBot.RunBot().GetAwaiter().GetResult);
        spotifyThread.Start();

        var twitchBotThread = new Thread(twitchBot.RunBot().GetAwaiter().GetResult);
        twitchBotThread.Start();

        return Task.Delay(-1);
    }
}