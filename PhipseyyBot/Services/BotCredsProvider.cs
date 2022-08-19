#nullable disable
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using PhipseyyBot.Common;
using PhipseyyBot.Common.Yml;
using Serilog;
using TwitchLib.Api.Core.Common;

namespace PhipseyyBot.Services;

public class BotCredsProvider
{
    private const string CredsFileName = "creds.yml";
    private const string CredsExampleFileName = "creds_example.yml";
    private string CredsPath { get; }
    private string CredsExamplePath { get; }

    private readonly BotCredentials _creds = new();
    private readonly IConfigurationRoot _config;


    private readonly object _reloadLock = new();
    private readonly IDisposable _changeToken;

    public BotCredsProvider(string credPath = null)
    {
        if (!string.IsNullOrWhiteSpace(credPath))
        {
            CredsPath = credPath;
            CredsExamplePath = Path.Combine(Path.GetDirectoryName(credPath), CredsExampleFileName);
        }
        else
        {
            CredsPath = Path.Combine(Directory.GetCurrentDirectory(), CredsFileName);
            CredsExamplePath = Path.Combine(Directory.GetCurrentDirectory(), CredsExampleFileName);
        }

        if (!File.Exists(CredsExamplePath))
            File.WriteAllText(CredsExamplePath, Yaml.Serializer.Serialize(_creds));

        if (!File.Exists(CredsPath))
        {
            Log.Warning(
                "{CredsPath} is missing. Attempting to load creds from environment variables prefixed with 'PhipseyyBot_'. Example is in {CredsExamplePath}",
                CredsPath,
                CredsExamplePath);
        }

        _config = new ConfigurationBuilder().AddYamlFile(CredsPath, false, true)
            .AddEnvironmentVariables("PhipseyyBot_")
            .Build();

        _changeToken = ChangeToken.OnChange(() => _config.GetReloadToken(), Reload);
    }

    public void Reload()
    {
        lock (_reloadLock)
        {
            _config.Bind(_creds);
            
            if (string.IsNullOrWhiteSpace(_creds.DiscordToken))
            {
                Log.Fatal("DiscordToken is missing from creds.yml or Environment variables.\nAdd it and restart the bot");
            }

            
        }
    }
    
    public IBotCredentials GetCreds()
    {
        lock (_reloadLock)
        {
            return _creds;
        }
    }
}