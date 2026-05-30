using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusLostAndFound.Web.Models.Enums;

namespace CampusLostAndFound.Web.Models.Entities;

public class Claim
{
    public int Id { get; set; }
    
    public int ItemId { get; set; }
    
    [Required]
    public string ClaimantUserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? ProofDescription { get; set; }
    
    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;
    
    [MaxLength(1000)]
    public string? AdminNotes { get; set; }
    
    public string? ReviewedByUserId { get; set; }
    
    public DateTime? DateReviewed { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("ItemId")]
    public virtual Item Item { get; set; } = null!;
    
    [ForeignKey("ClaimantUserId")]
    public virtual ApplicationUser Claimant { get; set; } = null!;
    
    [ForeignKey("ReviewedByUserId")]
    public virtual ApplicationUser? ReviewedBy { get; set; }
}
