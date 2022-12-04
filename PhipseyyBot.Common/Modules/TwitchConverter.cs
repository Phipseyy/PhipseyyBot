using PhipseyyBot.Common.Services;
using TwitchLib.Api;

namespace PhipseyyBot.Common.Modules;

public static class TwitchConverter
{
    public static string GetTwitchIdFromName(string twitchName)
    {
        var creds = new BotCredsProvider().GetCreds();
        var api = new TwitchAPI
        {
            Settings =
            {
                AccessToken = creds.TwitchAccessToken,
                ClientId = creds.TwitchClientId
            }
        };

        var users = api.Helix.Search.SearchChannelsAsync(twitchName, first: 100).Result;
        var twitchUser = users.Channels.SingleOrDefault(x => string.Equals(x.DisplayName, twitchName, StringComparison.CurrentCultureIgnoreCase));

        if (twitchUser == null)
            throw new Exception("Twitch user not found!");

        return twitchUser.Id;
    }
}