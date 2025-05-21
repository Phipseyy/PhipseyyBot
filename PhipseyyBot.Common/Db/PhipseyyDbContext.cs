#nullable disable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PhipseyyBot.Common.Db.Models;
using Swan;


namespace PhipseyyBot.Common.Db;

public class PhipseyyDbContext : DbContext
{
    private readonly string _dbPath = AppContext.BaseDirectory + "DB/PhipseyyBot.db";
    public virtual DbSet<GuildConfig> GuildConfigs { get; set; }
    public virtual DbSet<SpotifyConfig> SpotifyConfigs { get; set; }
    public virtual DbSet<TwitchConfig> TwitchConfigs { get; set; }


    public PhipseyyDbContext()
    {
        if (!Directory.Exists(Path.GetDirectoryName(_dbPath)))
            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath) ?? throw new InvalidOperationException());
    }
    
    public PhipseyyDbContext(DbContextOptions<PhipseyyDbContext> dbContextOptions = null)
    {
        if (!Directory.Exists(Path.GetDirectoryName(_dbPath)))
            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath) ?? throw new InvalidOperationException());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_dbPath}", x => { x.MigrationsAssembly("PhipseyyBot.Common"); });
        base.OnConfiguring(options);
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    
        modelBuilder.Entity<GuildConfig>()
            .HasMany(g => g.TwitchConfigs)
            .WithOne(t => t.GuildConfig)
            .HasForeignKey(t => t.GuildId)
            .OnDelete(DeleteBehavior.Cascade);
    
        modelBuilder.Entity<GuildConfig>()
            .HasOne(g => g.SpotifyConfig)
            .WithOne(s => s.GuildConfig)
            .HasForeignKey<SpotifyConfig>(s => s.GuildId)
            .OnDelete(DeleteBehavior.Cascade);
    
        modelBuilder.Entity<GuildConfig>()
            .HasIndex(g => g.GuildId)
            .IsUnique();
        
        modelBuilder.Entity<TwitchConfig>()
            .HasIndex(t => t.ChannelId);
    }

}