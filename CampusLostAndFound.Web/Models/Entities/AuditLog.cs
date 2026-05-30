using System.ComponentModel.DataAnnotations;

namespace CampusLostAndFound.Web.Models.Entities;

public class AuditLog
{
    public int Id { get; set; }
    
    public string? UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? EntityId { get; set; }
    
    public string? OldValues { get; set; }
    
    public string? NewValues { get; set; }
    
    [MaxLength(50)]
    public string? IpAddress { get; set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
