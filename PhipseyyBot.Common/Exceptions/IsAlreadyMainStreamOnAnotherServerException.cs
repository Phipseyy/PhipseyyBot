namespace PhipseyyBot.Common.Exceptions;

public class IsAlreadyMainStreamOnAnotherServerException : Exception
{
    public IsAlreadyMainStreamOnAnotherServerException(string username)
        : base($"{username} is already the Main Stream from another Server")
    {
    }
}