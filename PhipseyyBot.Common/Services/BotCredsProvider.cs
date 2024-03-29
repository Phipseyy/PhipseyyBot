﻿#nullable disable
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using PhipseyyBot.Common.Exceptions;
using PhipseyyBot.Common.Yml;
using Serilog;

namespace PhipseyyBot.Common.Services;

public interface IBotCredsProvider
{
    event EventHandler ConfigfileEdited;
    public IBotCredentials GetCreds();
    public void Reload();
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
                throw new FatalCredNotFoundException("DiscordToken");

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
    
    public IBotCredentials GetCreds()
    {
        lock (_reloadLock)
            return _creds;
    }
}