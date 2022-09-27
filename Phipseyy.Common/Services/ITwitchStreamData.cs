using Discord;

namespace Phipseyy.Common.Services;

public interface ITwitchStreamData
{
    string Username { get; }
    string Title { get; }
    string UrlToProfilePicture { get; }
    string UrlToPreview { get; }

    string Game { get; }


}