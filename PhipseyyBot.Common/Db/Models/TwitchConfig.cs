using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhipseyyBot.Common.Db.Models;

public class TwitchConfig
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public string ChannelId { get; set; }
    
    [Required]
    public string Username { get; set; }
    
    [Required]
    [ForeignKey("GuildConfig")]
    public ulong GuildId { get; set; }
    
    public bool MainStream { get; set; }
    public string? SpotifySr { get; set; }
    
    // Navigation property to GuildConfig
    public virtual GuildConfig GuildConfig { get; set; }
}