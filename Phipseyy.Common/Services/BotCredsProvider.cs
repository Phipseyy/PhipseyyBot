#nullable disable
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Phipseyy.Common.Exceptions;
using Phipseyy.Common.Yml;
using Serilog;
using SpotifyAPI.Web;

namespace Phipseyy.Common.Services;

public interface IBotCredsProvider
{
    public IBotCredentials GetCreds();
    public void Reload();

    public PKCETokenResponse GetSpotifyToken();

    public void OverrideSpotifyTokenData(PKCETokenResponse response);
    
    public void OverrideSpotifyTokenData(AuthorizationCodeRefreshResponse response);

}

public class BotCredsProvider : IBotCredsProvider
{
    private const string CredsFileName = "creds.yml";
    private string CredsPath { get; }

    public event EventHandler ConfigfileEdited;

    private readonly BotCredentials _creds = new();
    private readonly IConfigurationRoot _config;


    private readonly object _reloadLock = new();

    public BotCredsProvider(string credPath = null)
    {
        CredsPath = !string.IsNullOrWhiteSpace(credPath) ? credPath : Path.Combine(AppContext.BaseDirectory, CredsFileName);

        if (!File.Exists(CredsPath))
        {
            File.WriteAllText(CredsPath, Yaml.Serializer.Serialize(_creds));
            Log.Warning("{CredsPath} is missing. Created it myself but you gotta fill in the information on your own", CredsPath);
        }
        
        _config = new ConfigurationBuilder()
            .AddYamlFile(CredsPath, false, true)
            .Build();

        ChangeToken.OnChange(() => _config.GetReloadToken(), Reload);
        Reload();
    }
    
    public void Reload()
    {
        lock (_reloadLock)
        {
            _config.Bind(_creds);
            if (string.IsNullOrWhiteSpace(_creds.DiscordToken))
                throw new FatalCredNotFound("DiscordToken");

            if (string.IsNullOrWhiteSpace(_creds.DiscordStatus))
                Log.Warning("DiscordStatus is missing from creds.yml. The bot will not have a status message");
            
            if (string.IsNullOrWhiteSpace(_creds.TwitchUsername))
                Log.Warning("TwitchUsername is missing from creds.yml. Add it and restart the bot");

            if (string.IsNullOrWhiteSpace(_creds.TwitchAccessToken))
                Log.Warning("TwitchAccesstoken is missing from creds.yml. The bot will not have a status message");
            
            if (string.IsNullOrWhiteSpace(_creds.TwitchRefreshToken))
                Log.Warning("TwitchRefreshToken is missing from creds.yml. The bot will not have a status message");
            
            if (string.IsNullOrWhiteSpace(_creds.TwitchClientId))
                Log.Warning("TwitchClientId is missing from creds.yml. The bot will not have a status message");
            
            if (string.IsNullOrWhiteSpace(_creds.SpotifyClientId))
                Log.Warning("SpotifyClientId is missing from creds.yml.The bot will not have a status message");
        }
        ConfigfileEdited?.Invoke(this, EventArgs.Empty);
    }


    public void OverrideSettings(IBotCredentials creds)
    {
        lock (_reloadLock)
        {
            File.WriteAllText(CredsPath, Yaml.Serializer.Serialize(creds));
        }
    }

    public PKCETokenResponse GetSpotifyToken()
    {
        var token = new PKCETokenResponse();
        lock (_reloadLock)
        {
            token.Scope = _creds.SpScope;
            token.AccessToken = _creds.SpAccessToken;
            token.CreatedAt = _creds.SpCreatedAt;
            token.ExpiresIn = _creds.SpExpiresIn;
            token.RefreshToken = _creds.SpRefreshToken;
            token.TokenType = _creds.SpTokenType;
        }
        return token;
    }

    public void OverrideSpotifyTokenData(PKCETokenResponse response)
    {
        lock (_reloadLock)
        {
            _creds.SpScope = response.Scope;
            _creds.SpAccessToken = response.AccessToken;
            _creds.SpCreatedAt = response.CreatedAt;
            _creds.SpExpiresIn = response.ExpiresIn;
            _creds.SpRefreshToken = response.RefreshToken;
            _creds.SpTokenType = response.TokenType;
            File.WriteAllText(CredsPath, Yaml.Serializer.Serialize(_creds));
        }
    }

    public void OverrideSpotifyTokenData(AuthorizationCodeRefreshResponse response)
    {
        lock (_reloadLock)
        {
            _creds.SpScope = response.Scope;
            _creds.SpAccessToken = response.AccessToken;
            _creds.SpCreatedAt = response.CreatedAt;
            _creds.SpExpiresIn = response.ExpiresIn;
            _creds.SpRefreshToken = response.RefreshToken;
            _creds.SpTokenType = response.TokenType;
            File.WriteAllText(CredsPath, Yaml.Serializer.Serialize(_creds));
        }
    }
    
    public IBotCredentials GetCreds()
    {
        lock (_reloadLock)
            return _creds;
    }
}