#nullable disable
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PhipseyyBot.Common;
using PhipseyyBot.Common.Db;
using PhipseyyBot.Common.Db.Extensions;
using PhipseyyBot.Common.Modules;
using PhipseyyBot.Common.Services;
using PhipseyyBot.Spotify;
using Serilog;
using TwitchLib.Api;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using static System.DateTime;

namespace PhipseyyBot.Discord.Services.PubSub;

public class PubSubService
{
    private static Dictionary<SocketGuild, SpotifyBot> _spotifyClients;
    private static PhipseyyDbContext _dbContext;
    private static DiscordBot _discordBot;
    private static TwitchPubSub _pubSub;
    private static IBotCredentials _creds;
    public static bool IsConnected { get; private set; }
    
    
    public PubSubService(DiscordBot discordBot, IServiceProvider serviceProvider)
    {
        _dbContext = serviceProvider.GetRequiredService<PhipseyyDbContext>();
        _discordBot = discordBot;
        _spotifyClients = new Dictionary<SocketGuild, SpotifyBot>();
        _pubSub = new TwitchPubSub();

        _creds = new BotCredsProvider().GetCreds();
    }

    public async Task InitializePubSub()
    {
        _pubSub.OnPubSubServiceConnected += PubSub_OnServiceConnected;
        _pubSub.OnPubSubServiceClosed += PubSubOn_OnPubSubServiceClosed;
        _pubSub.OnStreamUp += PubSub_OnStreamUp;
        _pubSub.OnRewardRedeemed += PubSubOnOnRewardRedeemed;
        _pubSub.OnChannelPointsRewardRedeemed += PubSub_OnChannelPointsRewardRedeemed;
        _pubSub.OnLog += PubSub_OnLog;
        _pubSub.OnPubSubServiceError += PubSub_OnPubSubServiceError;
        
        _pubSub.Connect();
        
        await Task.Delay(-1);
    }


    /* --- Helpers --- */
    private static void LogTwitchPubSub(string message)
    {  
        Log.Information($"[TwitchPubSub] {Now:HH:mm:ss} {message}");
        
        if (message.Contains("RECONNECT"))
        {
            Log.Information($"[TwitchPubSub] {Now:HH:mm:ss} Trying to reconnect rn. Blame Phil if this doesnt work :)");
            _discordBot.SendGlobalLogMessage($"Twitch-PubSub: PubSub Service requested a reconnect");
        }
    }
    
    public static Task RestartService()
    {
        _pubSub.Disconnect();
        _pubSub.Connect();
        
        return Task.CompletedTask;
    }

    /* --- PubSub Events --- */
    
    private async void PubSub_OnServiceConnected(object sender, EventArgs e)
    {
        try
        {
            LogTwitchPubSub("---PubSub Connected!---");
            _discordBot.SendGlobalLogMessage("PubSub Service online!");

            var streams = _dbContext.GetListOfAllStreamIds();

            foreach (var id in streams)
                _pubSub.ListenToVideoPlayback(id);

            var mainStreams = _dbContext.GetListOfAllMainStreams();
            foreach (var mainStreamConfig in mainStreams)
                _pubSub.ListenToRewards(mainStreamConfig.ChannelId);

            _pubSub.SendTopics(_creds.TwitchAccessToken);
            IsConnected = true;
        }
        catch (Exception exception)
        {
            LogTwitchPubSub($" {exception.GetType()} // {exception.Message} // {exception.StackTrace}" +
                            $" // {exception.Source} // {exception.InnerException} // {exception.Data} // {exception.HelpLink}");
            await RestartService();
        }
    }
    
    private async void PubSubOn_OnPubSubServiceClosed(object sender, EventArgs e)
    {
        try
        {
            LogTwitchPubSub("PubSubService got closed! Restarting...");
            IsConnected = false;
            await RestartService();
        }
        catch (Exception exception)
        {
            LogTwitchPubSub($" {exception.GetType()} // {exception.Message} // {exception.StackTrace}" +
                            $" // {exception.Source} // {exception.InnerException} // {exception.Data} // {exception.HelpLink}");
        }
    }

    
    private async void PubSub_OnStreamUp(object sender, OnStreamUpArgs e)
    {
        try
        {
            LogTwitchPubSub("Stream is up now!");
            var api = new TwitchAPI
            {
                Settings =
                {
                    AccessToken = _creds.TwitchAccessToken,
                    ClientId = _creds.TwitchClientId
                }
            };

            var usersData = api.Helix.Channels.GetChannelInformationAsync(e.ChannelId, _creds.TwitchAccessToken).Result.Data.SingleOrDefault(x => x.BroadcasterId == e.ChannelId);
            var user = api.Helix.Search.SearchChannelsAsync(usersData!.BroadcasterName).Result.Channels.SingleOrDefault(x => x.DisplayName == usersData.BroadcasterName);
            var twitchData = new TwitchStreamData(user!.DisplayName,
                user!.Id,
                user.Title,
                user.ThumbnailUrl,
                user.GameName,
                user.StartedAt);

            await _discordBot.SendStreamNotification(twitchData);
        }
        catch (Exception exception)
        {
            LogTwitchPubSub($" {exception.GetType()} // {exception.Message} // {exception.StackTrace}" +
                            $" // {exception.Source} // {exception.InnerException} // {exception.Data} // {exception.HelpLink}");
            _discordBot.SendGlobalLogMessage($"OnStreamUp: {exception.Message}");
        }
    }
    
    private void PubSubOnOnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
    {
        var currentConfig = _dbContext.GetMainStreamOfChannel(e.ChannelId);
        
        if (e.RewardId.ToString() == currentConfig.SpotifySr && e.Status == "UNFULFILLED")
        {
            LogTwitchPubSub($"Song Request: {e.Message}");
            var client = _spotifyClients.FirstOrDefault(pair => pair.Key.Id == currentConfig.GuildId).Value;
            client.AddSongToQueue(e.Message);
        }
    }
    
    private void PubSub_OnChannelPointsRewardRedeemed(object sender, OnChannelPointsRewardRedeemedArgs e)
    {
        var currentConfig = _dbContext.GetMainStreamOfChannel(e.ChannelId);
        
        if (e.RewardRedeemed.Redemption.Reward.Id.Equals(currentConfig.SpotifySr))
        {
            LogTwitchPubSub($"Song Request: {e.RewardRedeemed.Redemption.UserInput}");
            var client = _spotifyClients.FirstOrDefault(pair => pair.Key.Id == currentConfig.GuildId).Value;
            client.AddSongToQueue(e.RewardRedeemed.Redemption.UserInput);
        }
    }
    
    private static void PubSub_OnLog(object sender, OnLogArgs e)
        => LogTwitchPubSub(e.Data);
    
    private void PubSub_OnPubSubServiceError(object sender, OnPubSubServiceErrorArgs e)
    {
        LogTwitchPubSub($"PubSubService error: {e.Exception.Message}");
        IsConnected = false;
        RestartService();
    }
    


    /* --- Features --- */

    public static void FollowStream(string twitchUser)
    {
        _pubSub.ListenToVideoPlayback(TwitchConverter.GetTwitchIdFromName(twitchUser));
        _pubSub.SendTopics(_creds.TwitchAccessToken);
    }


    public static void AddSpotifyClient(SocketGuild guild)
    {
        _spotifyClients.Add(guild, new SpotifyBot(guild.Id));
    }
    

    public static void StartSpotifyForGuild(ulong guildId)
    {
        var guildPair = _spotifyClients.FirstOrDefault(pair => pair.Key.Id == guildId);
        if (guildPair.Key == null)
            return;
        
        var threadSpotify = new Thread(guildPair.Value.RunBot().GetAwaiter().GetResult);
        threadSpotify.Start();
    }


    public static bool IsSpotifyActive(ulong guildId)
    {
        var (key, spotify) = _spotifyClients.FirstOrDefault(pair => pair.Key.Id == guildId);
        return key != null && spotify.IsActive();
    }

    public static void DeleteSpotifyConfig(ulong guildId)
    {
        var (key, _) = _spotifyClients.FirstOrDefault(pair => pair.Key.Id == guildId);
        _spotifyClients.Remove(key);
    }

    public static string GetSpotifyUsername(ulong guildId)
    {
        var (key, spotify) = _spotifyClients.FirstOrDefault(pair => pair.Key.Id == guildId && pair.Value.GetGuildId() == guildId);
        if (key == null)
            return "/";

        try
        {
            return spotify.GetUsername();
        }
        catch (Exception e)
        {
            LogTwitchPubSub($"GetSpotifyUsername {e.GetType()} // {e.Message} // {e.StackTrace}" +
                            $" // {e.Source} // {e.InnerException} // {e.Data} // {e.HelpLink}");
            return "Unable to load name";
        }
    }

}