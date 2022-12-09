namespace PhipseyyBot.Discord.Modules.Models;

public class Authorization
{
    public string Code { get; }
        
    public Authorization(string code)
    {
        Code = code;
    }
}