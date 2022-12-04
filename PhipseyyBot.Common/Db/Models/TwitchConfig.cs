#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhipseyyBot.Common.Db.Models;

public class TwitchConfig
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string ChannelId { get; set; }
    public string Username { get; set; }
    
    [ForeignKey("GuildId")]
    public ulong GuildId { get; set; }
    public bool MainStream { get; set; }
    public string SpotifySr { get; set; }

}