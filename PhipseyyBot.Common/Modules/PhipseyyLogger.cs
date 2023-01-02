using Serilog;

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
        => Log.Information("--- Discord Bot ~ By Philted ---");

}