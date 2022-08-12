using Phipseyy.Common;
using Phipseyy.Common.Modules;
using Serilog;
using Serilog.Core;

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