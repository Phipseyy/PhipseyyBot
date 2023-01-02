namespace PhipseyyBot.Common.Exceptions;

public class FatalCredNotFoundException : Exception
{
    public FatalCredNotFoundException(string type)
        : base($"[Fatal-Error] {type} is missing from creds.yml which is essential for the bot. Add it and restart the bot")
    {
    }
}