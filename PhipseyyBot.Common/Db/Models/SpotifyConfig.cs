﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhipseyyBot.Common.Db.Models;

public class SpotifyConfig
{
    [Key] 
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [ForeignKey("GuildConfig")]
    public ulong GuildId { get; set; }
    
    public string SpotifyClientId { get; set; }
    public string SpotifyClientSecret { get; set; }
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public string Scope { get; set; }
    public string RefreshToken { get; set; }
    public DateTime CreatedAt{ get; set; }
    public bool IsExpired { get; set; }
    
    // Navigation property to GuildConfig
    public virtual GuildConfig GuildConfig { get; set; }
    
    [NotMapped]
    public bool IsTokenExpired => DateTime.UtcNow > CreatedAt.AddSeconds(ExpiresIn);
}