#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EntityFrameworkCore.EncryptColumn.Attribute;

namespace PhipseyyBot.Common.Db.Models;

public class SpotifyConfig
{
    [Key] 
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public ulong GuildId { get; set; }
    
    public string SpotifyClientId { get; set; }
    
    [EncryptColumn]
    public string SpotifyClientSecret { get; set; }
    
    [EncryptColumn]
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public string Scope { get; set; }
    public string RefreshToken { get; set; }
    public DateTime CreatedAt{ get; set; }
    public bool IsExpired { get; set; }
}