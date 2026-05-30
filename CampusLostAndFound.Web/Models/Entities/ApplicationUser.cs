using Microsoft.AspNetCore.Identity;

namespace CampusLostAndFound.Web.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? StudentId { get; set; }
    public string? Department { get; set; }
    public string? ProfileImagePath { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<Item> ReportedItems { get; set; } = new List<Item>();
    public virtual ICollection<Item> ClaimedItems { get; set; } = new List<Item>();
    public virtual ICollection<Item> VerifiedItems { get; set; } = new List<Item>();
    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
    public virtual ICollection<Claim> ReviewedClaims { get; set; } = new List<Claim>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    
    public string FullName => $"{FirstName} {LastName}";
}
