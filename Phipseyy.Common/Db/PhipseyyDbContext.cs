﻿#nullable disable
using Microsoft.EntityFrameworkCore;
using Phipseyy.Common.Db.Models;

namespace Phipseyy.Common.Db;

public class PhipseyyDbContext : DbContext
{
    private readonly string _dbPath = Environment.CurrentDirectory + "\\DB\\PhipseyyBot.db";
    
    public DbSet<GuildConfig> GuildConfigs { get; set; }
    public DbSet<SpotifyConfig> SpotifyConfigs { get; set; }
    public DbSet<TwitchConfig> TwitchConfigs { get; set; }

    public PhipseyyDbContext()
    {
        if (!Directory.Exists(Path.GetDirectoryName(_dbPath)))
            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath) ?? throw new InvalidOperationException());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={_dbPath}");
}