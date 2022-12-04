using Microsoft.EntityFrameworkCore;
using PhipseyyBot.Common.Db;

namespace PhipseyyBot.Common.Services;

public static class DbService
{
    public static async Task SetupAsync()
    {
        var context = new PhipseyyDbContext();
        await context.Database.MigrateAsync();
    }
    
    private static PhipseyyDbContext GetDbContextInternal()
    {
        var context = new PhipseyyDbContext();
        var connection =  context.Database.GetDbConnection();
        connection.Open();
        using var com = connection.CreateCommand();
        com.CommandText = "PRAGMA synchronous=OFF";
        com.ExecuteNonQuery();

        return context;
    }

    public static PhipseyyDbContext GetDbContext()
        => GetDbContextInternal();


}