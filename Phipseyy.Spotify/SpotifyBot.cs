using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web;
using Newtonsoft.Json;
using Phipseyy.Common.Services;
using Serilog;
using static System.DateTime;
using static System.String;
using static SpotifyAPI.Web.Scopes;

namespace Phipseyy.Spotify;

public class SpotifyBot
{
    private static SpotifyClient _spotify = null!;
    private static string _credentialsPath = null!;
    private static string _clientId = null!;
    private static EmbedIOAuthServer _server = null!;

    public SpotifyBot(SettingsHandler settings, string path)
    { 
        _clientId = settings.SpotifyClientId;
        _credentialsPath = path;
        var uri = new Uri($"http://localhost:5000/callback");
        _server = new EmbedIOAuthServer(uri, 5000);
    }

    public async Task RunBot()
    {
        await Start();
        await Task.Delay(-1);
    }

    private static async Task Start()
    {
        if (File.Exists(_credentialsPath))
            await StartSpotify();
        else
            await StartAuthentication();
    }


    private static async Task StartSpotify()
    {
        var json = await File.ReadAllTextAsync(_credentialsPath);
        var token = JsonConvert.DeserializeObject<PKCETokenResponse>(json);

        var authenticator = new PKCEAuthenticator(_clientId, token!);
        // TODO: Combine spotifyCred with the config.JSON
        authenticator.TokenRefreshed += (_, tokenResponse) => File.WriteAllText(_credentialsPath, JsonConvert.SerializeObject(tokenResponse));

        var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(authenticator);

        _spotify = new SpotifyClient(config);

        var me = await _spotify.UserProfile.Current();
        LogSpotify($"Welcome {me.DisplayName} ({me.Id}), you're authenticated!");
    }

    private static async Task StartAuthentication()
    {
        var (verifier, challenge) = PKCEUtil.GenerateCodes();

        await _server.Start();
        _server.AuthorizationCodeReceived += async (_, response) =>
        {
            await _server.Stop();
            var token = await new OAuthClient().RequestToken(
                new PKCETokenRequest(_clientId, response.Code, _server.BaseUri, verifier)
            );

            await File.WriteAllTextAsync(_credentialsPath, JsonConvert.SerializeObject(token));
            await Start();
        };

        var request = new LoginRequest(_server.BaseUri, _clientId, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256",
            Scope = new List<string> { UserReadEmail, UserReadPrivate, PlaylistReadPrivate, PlaylistReadCollaborative, UserModifyPlaybackState, UserReadCurrentlyPlaying}
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
            if (url.Contains("open.spotify"))
            {
                PlayerAddToQueueRequest playerAddToQueueRequest = new PlayerAddToQueueRequest(UrlToUri(url));
                _spotify.Player.AddToQueue(playerAddToQueueRequest);
            }
        }
        catch (Exception ex)
        {
            LogSpotify(ex.Message);
        }

    }

    public string GetCurrentSong()
    {
        var request = _spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());
        var fullTrack = (FullTrack)request.Result.Item;
        return $"{fullTrack.Name} - {Join(',', fullTrack.Artists.ToList().Select(a=>a.Name).ToArray())} {fullTrack.ExternalUrls["spotify"]}";
    }
}