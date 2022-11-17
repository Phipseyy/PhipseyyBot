using System.ComponentModel.DataAnnotations;

namespace Phipseyy.Common.Db.Models;

public class GuildConfig
{
    [Key]
    public ulong GuildId { get; set; }
    public ulong LogChannel { get; set; }
    public ulong StreamNotificationChannel { get; set; }
}