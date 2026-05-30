using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CampusLostAndFound.Web.Models.Entities;

namespace CampusLostAndFound.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Location> Locations { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;
    public DbSet<ItemImage> ItemImages { get; set; } = null!;
    public DbSet<Claim> Claims { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Category configuration
        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });
        
        // Location configuration
        builder.Entity<Location>(entity =>
        {
            entity.HasIndex(e => e.Name);
        });
        
        // Item configuration
        builder.Entity<Item>(entity =>
        {
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ItemType);
            entity.HasIndex(e => e.DateOccurred);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.LocationId);
            
            entity.HasOne(e => e.ReportedBy)
                .WithMany(u => u.ReportedItems)
                .HasForeignKey(e => e.ReportedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.ClaimedBy)
                .WithMany(u => u.ClaimedItems)
                .HasForeignKey(e => e.ClaimedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.VerifiedBy)
                .WithMany(u => u.VerifiedItems)
                .HasForeignKey(e => e.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Location)
                .WithMany(l => l.Items)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ItemImage configuration
        builder.Entity<ItemImage>(entity =>
        {
            entity.HasIndex(e => e.ItemId);
            
            entity.HasOne(e => e.Item)
                .WithMany(i => i.Images)
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Claim configuration
        builder.Entity<Claim>(entity =>
        {
            entity.HasIndex(e => e.ItemId);
            entity.HasIndex(e => e.ClaimantUserId);
            entity.HasIndex(e => e.Status);
            
            entity.HasOne(e => e.Item)
                .WithMany(i => i.Claims)
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Claimant)
                .WithMany(u => u.Claims)
                .HasForeignKey(e => e.ClaimantUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.ReviewedBy)
                .WithMany(u => u.ReviewedClaims)
                .HasForeignKey(e => e.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Notification configuration
        builder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsRead);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.RelatedItem)
                .WithMany()
                .HasForeignKey(e => e.RelatedItemId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.RelatedClaim)
                .WithMany()
                .HasForeignKey(e => e.RelatedClaimId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        // AuditLog configuration
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.EntityType);
        });
    }
}
