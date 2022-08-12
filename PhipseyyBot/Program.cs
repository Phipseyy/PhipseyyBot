using Phipseyy.Common.Modules;

namespace PhipseyyBot;

public class Program
{
    
    public static Task Main(string[] args) => new Program().MainAsync();

    private async Task MainAsync()
    {
        PhipseyLogger.SetupLogger("/Logs/Bot.log");
        await Bot.StartupBot();
    }
}