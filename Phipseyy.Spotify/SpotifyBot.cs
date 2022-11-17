#nullable disable
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web;
using Phipseyy.Common.Db;
using Phipseyy.Common.Db.Extensions;
using Phipseyy.Common.Services;
using Serilog;
using static System.DateTime;
using static System.String;
using static SpotifyAPI.Web.Scopes;

namespace Phipseyy.Spotify;

public class SpotifyBot
{
    private static ulong _guildId;
    private static PhipseyyDbContext _dbContext;
    
    private static SpotifyClient _spotify = null!;
    private static EmbedIOAuthServer _server = null!;

    public SpotifyBot(ulong guildId)
    {
        _dbContext = DbService.GetDbContext();
        _guildId = guildId;
        var creds = new BotCredsProvider().GetCreds();
        var uri = new Uri($"http://{creds.ServerIp}:5000/callback");
        _server = new EmbedIOAuthServer(uri, 5000);
    }

    public async Task RunBot()
    {
        await Start();
        await Task.Delay(-1);
    }

    private static async Task Start()
    {
        var spotifyConfig = _dbContext.SpotifyConfigs.FirstOrDefault(config => config.GuildId == _guildId);
        if (spotifyConfig != null)
            await StartSpotify();
        else
            await StartAuthentication();
    }
    
    private static async Task StartSpotify()
    {
        try
        {
            var spotifyConfig = _dbContext.SpotifyConfigs.FirstOrDefault(config => config.GuildId == _guildId);
            if (spotifyConfig == null) throw new Exception("No Spotify Config found in DB");

            var token = _dbContext.GetSpotifyToken(_guildId);

            var authenticator = new PKCEAuthenticator(spotifyConfig.SpotifyClientId, token!);
            authenticator.TokenRefreshed += (_, response) => _dbContext.SetSpotifyDataToDb(_guildId, response, spotifyConfig.SpotifyClientId, spotifyConfig.SpotifyClientSecret);

            var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(authenticator);
            _spotify = new SpotifyClient(config);

            var me = await _spotify.UserProfile.Current();
            LogSpotify($"Welcome {me.DisplayName} ({me.Id}), you're authenticated!");
        }
        catch (APIUnauthorizedException e)
        {
            LogSpotify(e.Message);
            LogSpotify("WE NEED A NEW TOKEN - TRYING TO CREATE ONE");
            var spotifyConfig = _dbContext.SpotifyConfigs.FirstOrDefault(config => config.GuildId == _guildId);
            if (spotifyConfig == null) throw new Exception("No Spotify Config found in DB");
            
            var newResponse = await new OAuthClient().RequestToken(
                new AuthorizationCodeRefreshRequest(spotifyConfig.SpotifyClientId, spotifyConfig.SpotifyClientSecret,
                    spotifyConfig.RefreshToken)
            );

            _dbContext.SetSpotifyDataToDb(_guildId, newResponse, spotifyConfig.SpotifyClientId, spotifyConfig.SpotifyClientSecret);

            _spotify = new SpotifyClient(newResponse.AccessToken);
            var me = await _spotify.UserProfile.Current();
            LogSpotify($"Welcome {me.DisplayName} ({me.Id}), you're authenticated!");
        }
        catch (Exception e)
        {
            LogSpotify(e.Message);
        }
    }

    private static async Task StartAuthentication()
    {
        var (verifier, challenge) = PKCEUtil.GenerateCodes();
        var spotifyConfig = _dbContext.SpotifyConfigs.FirstOrDefault(config => config.GuildId == _guildId);
        if (spotifyConfig == null) throw new Exception("No Spotify Config found in DB");
        
        await _server.Start();
        _server.AuthorizationCodeReceived += async (_, response) =>
        {
            await _server.Stop();
            var token = await new OAuthClient().RequestToken(new PKCETokenRequest(spotifyConfig.SpotifyClientId, response.Code, _server.BaseUri, verifier));
            _dbContext.SetSpotifyDataToDb(_guildId, token, spotifyConfig.SpotifyClientId, spotifyConfig.SpotifyClientSecret);
            await Start();
        };

        var request = new LoginRequest(_server.BaseUri, spotifyConfig.SpotifyClientId, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256",
            Scope = new List<string>
            {
                UserReadEmail, UserReadPrivate, PlaylistReadPrivate, PlaylistReadCollaborative, UserModifyPlaybackState, UserReadCurrentlyPlaying
            }
        };

        var uri = request.ToUri();
        try
        {
            BrowserUtil.Open(uri);
        }
        catch (Exception)
        {
            LogSpotify("Unable to open URL, manually open: " + uri);
        }
    }

    private static void LogSpotify(string message)
        => Log.Information($"[Spotify] {Now:HH:mm:ss} {message}");

    private static string UrlToUri(string url)
    {
        //SONG: https://open.spotify.com/track/2pyxICfn3Mu743C1GqjwpI?si=855fb73a4aa94639
        // URI: spotify:track:2pyxICfn3Mu743C1GqjwpI
        var uri = new Uri(url);
        return "spotify:track:" + uri.Segments[2];
    }

    public void AddSongToQueue(string url)
    {
        try
        {
            if (!url.Contains("open.spotify")) 
                return;
            var playerAddToQueueRequest = new PlayerAddToQueueRequest(UrlToUri(url));
            _spotify.Player.AddToQueue(playerAddToQueueRequest);
        }
        catch (Exception ex)
        {
            LogSpotify(ex.Message);
        }
    }

    public static string GetCurrentSong()
    {
        try
        {
            var request = _spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());
            var fullTrack = (FullTrack)request.Result.Item;
            return
                $"{fullTrack.Name} - {Join(',', fullTrack.Artists.ToList().Select(a => a.Name).ToArray())} {fullTrack.ExternalUrls["spotify"]}";
        }
        catch (Exception e)
        {
            LogSpotify(e.Message);
            return "";
        }
    }

    public bool IsActive()
    {
        try
        {
            var me = _spotify.UserProfile.Current();
            return true;
        }
        catch (APIUnauthorizedException e)
        {
            return false;
        }
    }

}