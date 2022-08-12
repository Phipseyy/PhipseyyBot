using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace Phipseyy.Common.Services;

public class SettingsHandler
{
    private readonly ConfigurationBuilder _config;
    public string DiscordToken { get; private set; }
    public string DiscordStatus { get; private set; }
    public string TwitchUsername { get; private set; }
    public string TwitchId { get; private set; }
    public string TwitchAccessToken { get; private set; }
    public string TwitchRefreshToken { get; private set; }
    public string TwitchClientId { get; private set; }
    public string SpotifyClientId { get; private set; }

    public SettingsHandler(string configPath)
    {
        _config = new ConfigurationBuilder();
        _config.SetBasePath(AppContext.BaseDirectory);
        _config.AddJsonFile("config.json");
        _config.Build();


        using (var r = new StreamReader(configPath))
        {
            var json = r.ReadToEnd();
            var items = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            DiscordToken = items["DiscordToken"];
            DiscordStatus = items["DiscordStatus"];
            TwitchUsername = items["TwitchUsername"];
            TwitchId = items["TwitchID"];
            TwitchAccessToken = items["TwitchAccesstoken"];
            TwitchRefreshToken = items["TwitchRefreshToken"];
            TwitchClientId = items["TwitchClientID"];
            SpotifyClientId = items["SpotifyClientID"];
        }
    }

    //TODO: Handling Errors / Mistakes


}