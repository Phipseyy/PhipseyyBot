#nullable disable
using EntityFrameworkCore.EncryptColumn.Extension;
using EntityFrameworkCore.EncryptColumn.Interfaces;
using EntityFrameworkCore.EncryptColumn.Util;
using Microsoft.EntityFrameworkCore;
using PhipseyyBot.Common.Db.Models;
using PhipseyyBot.Common.Services;


namespace PhipseyyBot.Common.Db;

public class PhipseyyDbContext : DbContext
{
    private readonly string _dbPath = AppContext.BaseDirectory + "DB/PhipseyyBot.db";

    private readonly IEncryptionProvider _provider;
    
    public DbSet<GuildConfig> GuildConfigs { get; set; }
    public DbSet<SpotifyConfig> SpotifyConfigs { get; set; }
    public DbSet<TwitchConfig> TwitchConfigs { get; set; }

    public PhipseyyDbContext()
    {
        var creds = new BotCredsProvider().GetCreds();
        _provider = new GenerateEncryptionProvider(creds.EncryptionKey);

        if (!Directory.Exists(Path.GetDirectoryName(_dbPath)))
            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath) ?? throw new InvalidOperationException());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_dbPath}");
        options.EnableSensitiveDataLogging();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseEncryption(_provider);
        base.OnModelCreating(modelBuilder);
    }
}