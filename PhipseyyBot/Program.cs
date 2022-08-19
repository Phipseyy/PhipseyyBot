using Phipseyy.Common.Modules;
using Serilog;

namespace PhipseyyBot;

public static class Program
{
    
    public static Task Main() => MainAsync();

    private static async Task MainAsync()
    {
        try
        {
            PhipseyyLogger.SetupLogger("/Logs/Bot.log");
            await Bot.StartupBot();
        }
        catch (Exception e)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            Log.Fatal(e.Message);
            await Task.Delay(-1);
        }
    }
}