using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusLostAndFound.Web.Models.Enums;

namespace CampusLostAndFound.Web.Models.Entities;

public class Item
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    
    public int LocationId { get; set; }
    
    [MaxLength(500)]
    public string? LocationDetails { get; set; }
    
    public ItemType ItemType { get; set; }
    
    public ItemStatus Status { get; set; } = ItemStatus.Open;
    
    [Required]
    public DateTime DateOccurred { get; set; }
    
    [Required]
    public string ReportedByUserId { get; set; } = string.Empty;
    
    public string? ClaimedByUserId { get; set; }
    
    public string? VerifiedByUserId { get; set; }
    
    [MaxLength(1000)]
    public string? VerificationNotes { get; set; }
    
    public DateTime? DateClaimed { get; set; }
    
    public DateTime? DateVerified { get; set; }
    
    public DateTime? DateReturned { get; set; }
    
    public bool IsVerified { get; set; } = false;
    
    public int ViewCount { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("CategoryId")]
    public virtual Category Category { get; set; } = null!;
    
    [ForeignKey("LocationId")]
    public virtual Location Location { get; set; } = null!;
    
    [ForeignKey("ReportedByUserId")]
    public virtual ApplicationUser ReportedBy { get; set; } = null!;
    
    [ForeignKey("ClaimedByUserId")]
    public virtual ApplicationUser? ClaimedBy { get; set; }
    
    [ForeignKey("VerifiedByUserId")]
    public virtual ApplicationUser? VerifiedBy { get; set; }
    
    public virtual ICollection<ItemImage> Images { get; set; } = new List<ItemImage>();
    
    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
}
