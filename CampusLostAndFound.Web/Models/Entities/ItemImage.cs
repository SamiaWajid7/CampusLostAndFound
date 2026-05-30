using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusLostAndFound.Web.Models.Entities;

public class ItemImage
{
    public int Id { get; set; }
    
    public int ItemId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;
    
    public bool IsPrimary { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("ItemId")]
    public virtual Item Item { get; set; } = null!;
}
