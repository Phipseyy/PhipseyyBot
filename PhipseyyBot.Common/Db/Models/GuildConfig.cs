#nullable disable
using System.ComponentModel.DataAnnotations;

namespace PhipseyyBot.Common.Db.Models;

public class GuildConfig
{
    [Key]
    public ulong GuildId { get; set; }
    public ulong LogChannel { get; set; }
    public ulong LiveChannel { get; set; }
}