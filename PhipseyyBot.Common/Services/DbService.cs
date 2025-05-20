using Microsoft.EntityFrameworkCore;
using PhipseyyBot.Common.Db;
using Serilog;

namespace PhipseyyBot.Common.Services;

public static class DbService
{
    public static async Task SetupAsync()
    {
        var context = new PhipseyyDbContext();

        Log.Information("Checking for pending migrations");
    
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            Log.Information("Migrating Database");
            await context.Database.MigrateAsync();
            Log.Information("Database migrated");
        }
        else
        {
            Log.Information("Database is up to date");
        }
    }

    private static PhipseyyDbContext GetDbContextInternal()
    {
        var context = new PhipseyyDbContext();
        var connection = context.Database.GetDbConnection();
        connection.Open();
        using var com = connection.CreateCommand();
        com.CommandText = "PRAGMA synchronous=OFF";
        com.ExecuteNonQuery();

        return context;
    }

    public static PhipseyyDbContext GetDbContext()
        => GetDbContextInternal();
}