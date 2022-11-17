#nullable disable
using Discord.WebSocket;
using Phipseyy.Spotify;

namespace Phipseyy.Discord.Services;

public class PubSubService
{
    private struct Pair
    {
        public PubSub PubSub;
        public SpotifyBot SpotifyBot;
    }
    
    private static Dictionary<SocketGuild, Pair> _guildServices;
    //Not used yet
    //private static PhipseyyDbContext _dbContext;
    private static DiscordBot _discordBot;

    private static IServiceProvider _service;
    

    public PubSubService(DiscordBot discordBot, IServiceProvider serviceProvider)
    {
        _guildServices = new Dictionary<SocketGuild, Pair>();
        _service = serviceProvider;
        _discordBot = discordBot;
    }

    public static void AddGuildCommand(SocketGuild guild)
    {
        var spotifyBot = new SpotifyBot(guild.Id);
        var pubSub = new PubSub(_discordBot, spotifyBot, guild.Id, _service);
        var pair = new Pair
        {
            PubSub = pubSub,
            SpotifyBot = spotifyBot
        };
        _guildServices.Add(guild, pair);
    }

    public void AddGuild(SocketGuild guild)
        => AddGuildCommand(guild);

    public static void StartServiceForGuildCommand(ulong guildId)
    {
        var guildPair = _guildServices.FirstOrDefault(pair => pair.Key.Id == guildId);
        if (guildPair.Key == null)
            return;
        
        var threadPubSub = new Thread(guildPair.Value.PubSub.InitializePubSub().GetAwaiter().GetResult);
        threadPubSub.Start();
    }

    public void StartServiceForGuild(ulong guildId)
        => StartServiceForGuildCommand(guildId);

    public static bool IsSpotifyActive(ulong guildId)
    {
        var guildPair = _guildServices.FirstOrDefault(pair => pair.Key.Id == guildId);
        if (guildPair.Key == null)
            return false;
        
        var spotify = guildPair.Value.SpotifyBot;
        return spotify.IsActive();
    }

}