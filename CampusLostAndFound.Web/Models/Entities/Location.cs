using System.ComponentModel.DataAnnotations;

namespace CampusLostAndFound.Web.Models.Entities;

public class Location
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Building { get; set; }
    
    [MaxLength(50)]
    public string? Floor { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
