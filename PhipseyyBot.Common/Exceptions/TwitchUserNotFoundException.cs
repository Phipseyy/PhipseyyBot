namespace PhipseyyBot.Common.Exceptions;

public class TwitchUserNotFoundException : Exception
{
    public TwitchUserNotFoundException(string username)
        : base($"{username} could not be found")
    {
    }
}