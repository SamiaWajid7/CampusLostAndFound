using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusLostAndFound.Web.Models.Enums;

namespace CampusLostAndFound.Web.Models.Entities;

public class Notification
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    public NotificationType Type { get; set; }
    
    public int? RelatedItemId { get; set; }
    
    public int? RelatedClaimId { get; set; }
    
    public bool IsRead { get; set; } = false;
    
    public DateTime? ReadAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;
    
    [ForeignKey("RelatedItemId")]
    public virtual Item? RelatedItem { get; set; }
    
    [ForeignKey("RelatedClaimId")]
    public virtual Claim? RelatedClaim { get; set; }
}
