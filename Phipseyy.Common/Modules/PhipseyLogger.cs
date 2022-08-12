﻿using System;
using Serilog;

namespace Phipseyy.Common.Modules;

public class PhipseyLogger
{
    public static void SetupLogger(string OutputDir)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate:"{Message}{NewLine}{Exception}")
            .WriteTo.File(AppContext.BaseDirectory+OutputDir, rollingInterval: RollingInterval.Day,
                outputTemplate:"[{Timestamp:yyyy.MM.dd}] - {Message}{NewLine}{Exception}")
            .CreateLogger();

        SendStartupMessage();
    }

    private static void SendStartupMessage()
        => Log.Information("--- Discord Bot ~ By Phipseyy ---");


}