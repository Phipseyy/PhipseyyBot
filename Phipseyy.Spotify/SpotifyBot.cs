using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web;
using Phipseyy.Common;
using Phipseyy.Common.Services;
using Serilog;
using static System.DateTime;
using static System.String;
using static SpotifyAPI.Web.Scopes;

namespace Phipseyy.Spotify;

public class SpotifyBot
{
    private static SpotifyClient _spotify = null!;
    private static EmbedIOAuthServer _server = null!;

    private static IBotCredsProvider _credsProvider = null!;
    private static IBotCredentials _creds = null!;

    public SpotifyBot()
    {
        _credsProvider = new BotCredsProvider();
        _creds = _credsProvider.GetCreds();
        var uri = new Uri("http://localhost:5000/callback");
        _server = new EmbedIOAuthServer(uri, 5000);
    }

    public async Task RunBot()
    {
        await Start();
        await Task.Delay(-1);
    }

    private static async Task Start()
    {
        var currentToken = _creds.SpAccessToken;
        if (!IsNullOrEmpty(currentToken))
            await StartSpotify();
        else
            await StartAuthentication();
    }


    private static async Task StartSpotify()
    {
        try
        {
            var token = _credsProvider.GetSpotifyToken();

            var authenticator = new PKCEAuthenticator(_creds.SpotifyClientId, token!);
            authenticator.TokenRefreshed += AuthenticatorOnTokenRefreshed;
            var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(authenticator);

            _spotify = new SpotifyClient(config);

            var me = await _spotify.UserProfile.Current();
            LogSpotify($"Welcome {me.DisplayName} ({me.Id}), you're authenticated!");
        }
        catch (APIUnauthorizedException e)
        {
            LogSpotify(e.Message);
            LogSpotify("WE NEED A NEW TOKEN - TRYING TO CREATE ONE");
            var newResponse = await new OAuthClient().RequestToken(
                new AuthorizationCodeRefreshRequest(_creds.SpotifyClientId, _creds.SpotifyClientSecret, _creds.SpRefreshToken)
            );
            
            _credsProvider.OverrideSpotifyTokenData(newResponse);

            _spotify = new SpotifyClient(newResponse.AccessToken);
            var me = await _spotify.UserProfile.Current();
            LogSpotify($"Welcome {me.DisplayName} ({me.Id}), you're authenticated!");
        }
        catch (Exception e)
        {
            LogSpotify(e.Message);
            throw;
        }
    }

    private static void AuthenticatorOnTokenRefreshed(object? sender, PKCETokenResponse e)
    {
        _credsProvider.OverrideSpotifyTokenData(e);
    }

    private static async Task StartAuthentication()
    {
        var (verifier, challenge) = PKCEUtil.GenerateCodes();

        await _server.Start();
        _server.AuthorizationCodeReceived += async (_, response) =>
        {
            await _server.Stop();
            var token = await new OAuthClient().RequestToken(
                new PKCETokenRequest(_creds.SpotifyClientId, response.Code, _server.BaseUri, verifier)
            );

            _credsProvider.OverrideSpotifyTokenData(token); 
            await Start();
        };

        var request = new LoginRequest(_server.BaseUri, _creds.SpotifyClientId, LoginRequest.ResponseType.Code)
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
        try
        {
            var request = _spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());
            var fullTrack = (FullTrack)request.Result.Item;
            return $"{fullTrack.Name} - {Join(',', fullTrack.Artists.ToList().Select(a=>a.Name).ToArray())} {fullTrack.ExternalUrls["spotify"]}";
        }
        catch (Exception e)
        {
            LogSpotify(e.Message);
            return "";
        }
    }
}