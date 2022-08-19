using Microsoft.Extensions.Configuration;
using PhipseyyBot.Common;
using PhipseyyBot.Common.Yml;
using Serilog;

namespace PhipseyyBot.Services;

public class BotCredsProvider
{
    private const string CredsFileName = "creds.yml";
    private const string CredsExampleFileName = "creds_example.yml";
    private string? CredsPath { get; }
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

        try
        {
            if (!File.Exists(CredsExamplePath))
                File.WriteAllText(CredsExamplePath, Yaml.Serializer.Serialize(_creds));
        }
        catch
        {
            // this can fail in docker containers
        }

        MigrateCredentials();

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

        //_changeToken = ChangeToken.OnChange(() => _config.GetReloadToken(), Reload);
    }

    public void MigrateCredentials()
    {
        
    }

}