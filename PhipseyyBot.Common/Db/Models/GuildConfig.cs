#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhipseyyBot.Common.Db.Models;

public class GuildConfig
{
    [Key] 

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public ulong GuildId { get; set; }
    public ulong LogChannel { get; set; }
    public ulong LiveChannel { get; set; }
}