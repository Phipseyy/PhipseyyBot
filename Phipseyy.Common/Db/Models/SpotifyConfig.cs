using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Phipseyy.Common.Db.Models;

public class SpotifyConfig
{
    [Key] 
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
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
}