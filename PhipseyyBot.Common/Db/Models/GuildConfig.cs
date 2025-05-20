#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhipseyyBot.Common.Db.Models;

public class GuildConfig
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public ulong GuildId { get; set; }
    
    public ulong LogChannel { get; set; }
    public ulong LiveChannel { get; set; }
    public ulong PartnerChannel { get; set; }
    
    public string MainStreamNotification { get; set; }
    public string PartnerStreamNotification { get; set; }
    
    public bool SendDebugMessages { get; set; }
    
    // Navigation properties
    public virtual SpotifyConfig SpotifyConfig { get; set; }
    public virtual ICollection<TwitchConfig> TwitchConfigs { get; set; } = new List<TwitchConfig>();
}