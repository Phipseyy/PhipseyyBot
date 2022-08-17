using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Phipseyy.Common.Exceptions;

namespace Phipseyy.Common.Services;

public class SettingsHandler
{
    public string DiscordToken { get; }
    public string DiscordStatus { get; }
    public string TwitchUsername { get; }
    public string TwitchId { get; }
    public string TwitchAccessToken { get; }
    public string SpotifyClientId { get; }

    public SettingsHandler(string configPath)
    {
        var config = new ConfigurationBuilder();
        config.SetBasePath(AppContext.BaseDirectory);
            
        if (File.Exists(AppContext.BaseDirectory))
            config.AddJsonFile("config.json");
        else
            throw new ConfigFileNotFound();
            
        config.Build();


        using (var r = new StreamReader(configPath))
        { 
            var json = r.ReadToEnd();
            var items = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            DiscordToken = items!["DiscordToken"];
            DiscordStatus = items["DiscordStatus"];
            TwitchUsername = items["TwitchUsername"];
            TwitchId = items["TwitchID"];
            TwitchAccessToken = items["TwitchAccesstoken"];
            SpotifyClientId = items["SpotifyClientID"];
        }
        
    }

    //TODO: Handling Errors / Mistakes


}