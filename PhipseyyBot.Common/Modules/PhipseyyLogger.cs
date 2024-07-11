using Microsoft.Extensions.Logging;
using Serilog;
using TwitchLib.PubSub;

namespace PhipseyyBot.Common.Modules;

public static class PhipseyyLogger
{
    public static void SetupLogger(string outputDir)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate:"{Message}{NewLine}{Exception}")
            .WriteTo.File(AppContext.BaseDirectory+outputDir, rollingInterval: RollingInterval.Day,
                outputTemplate:"[{Timestamp:yyyy.MM.dd}] - {Message}{NewLine}{Exception}")
            .CreateLogger();
    
        SendStartupMessage();
    }
    
    private static void SendStartupMessage()
        => Log.Information("--- Discord Bot ~ By Phipseyy ---");

    public static ILogger<TwitchPubSub> GetTwitchLogger()
    {
        // Create a LoggerFactory and add the global Serilog logger to it
        var factory = LoggerFactory.Create(builder => builder.AddSerilog(Log.Logger));
        
        // Create a logger for the specified category
        var logger = factory.CreateLogger<TwitchPubSub>();

        return logger;
    }

}