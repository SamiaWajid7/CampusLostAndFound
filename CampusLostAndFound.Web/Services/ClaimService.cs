using Microsoft.EntityFrameworkCore;
using CampusLostAndFound.Web.Data;
using CampusLostAndFound.Web.Models.DTOs;
using CampusLostAndFound.Web.Models.Entities;
using CampusLostAndFound.Web.Models.Enums;

namespace CampusLostAndFound.Web.Services;

public interface IClaimService
{
    Task<List<ClaimDto>> GetClaimsForItemAsync(int itemId);
    Task<List<ClaimDto>> GetUserClaimsAsync(string userId);
    Task<List<ClaimDto>> GetPendingClaimsAsync();
    Task<ClaimDto?> GetClaimByIdAsync(int id);
    Task<ClaimDto> CreateClaimAsync(CreateClaimDto dto, string userId);
    Task<ClaimDto?> ReviewClaimAsync(int id, ReviewClaimDto dto, string adminUserId);
    Task<bool> CancelClaimAsync(int id, string userId);
}

public class ClaimService : IClaimService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    
    public ClaimService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }
    
    public async Task<List<ClaimDto>> GetClaimsForItemAsync(int itemId)
    {
        return await _context.Claims
            .Include(c => c.Item)
            .Include(c => c.Claimant)
            .Include(c => c.ReviewedBy)
            .Where(c => c.ItemId == itemId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }
    
    public async Task<List<ClaimDto>> GetUserClaimsAsync(string userId)
    {
        return await _context.Claims
            .Include(c => c.Item)
            .Include(c => c.Claimant)
            .Include(c => c.ReviewedBy)
            .Where(c => c.ClaimantUserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }
    
    public async Task<List<ClaimDto>> GetPendingClaimsAsync()
    {
        return await _context.Claims
            .Include(c => c.Item)
            .Include(c => c.Claimant)
            .Where(c => c.Status == ClaimStatus.Pending)
            .OrderBy(c => c.CreatedAt)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }
    
    public async Task<ClaimDto?> GetClaimByIdAsync(int id)
    {
        var claim = await _context.Claims
            .Include(c => c.Item)
            .Include(c => c.Claimant)
            .Include(c => c.ReviewedBy)
            .FirstOrDefaultAsync(c => c.Id == id);
        
        return claim != null ? MapToDto(claim) : null;
    }
    
    public async Task<ClaimDto> CreateClaimAsync(CreateClaimDto dto, string userId)
    {
        // Check if user already has a pending claim for this item
        var existingClaim = await _context.Claims
            .FirstOrDefaultAsync(c => c.ItemId == dto.ItemId && 
                                      c.ClaimantUserId == userId && 
                                      c.Status == ClaimStatus.Pending);
        
        if (existingClaim != null)
        {
            throw new InvalidOperationException("You already have a pending claim for this item.");
        }
        
        var item = await _context.Items.FindAsync(dto.ItemId);
        if (item == null)
        {
            throw new InvalidOperationException("Item not found.");
        }
        
        if (item.ReportedByUserId == userId)
        {
            throw new InvalidOperationException("You cannot claim your own item.");
        }
        
        var claim = new Claim
        {
            ItemId = dto.ItemId,
            ClaimantUserId = userId,
            Description = dto.Description,
            ProofDescription = dto.ProofDescription,
            Status = ClaimStatus.Pending
        };
        
        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();
        
        // Notify the item reporter
        await _notificationService.CreateNotificationAsync(
            item.ReportedByUserId,
            "New Claim on Your Item",
            $"Someone has submitted a claim for your item '{item.Title}'.",
            NotificationType.ClaimUpdate,
            item.Id,
            claim.Id);
        
        return (await GetClaimByIdAsync(claim.Id))!;
    }
    
    public async Task<ClaimDto?> ReviewClaimAsync(int id, ReviewClaimDto dto, string adminUserId)
    {
        var claim = await _context.Claims
            .Include(c => c.Item)
            .FirstOrDefaultAsync(c => c.Id == id);
        
        if (claim == null) return null;
        
        claim.Status = dto.Status;
        claim.AdminNotes = dto.AdminNotes;
        claim.ReviewedByUserId = adminUserId;
        claim.DateReviewed = DateTime.UtcNow;
        claim.UpdatedAt = DateTime.UtcNow;
        
        // If approved, update the item status
        if (dto.Status == ClaimStatus.Approved)
        {
            claim.Item.Status = ItemStatus.Claimed;
            claim.Item.ClaimedByUserId = claim.ClaimantUserId;
            claim.Item.DateClaimed = DateTime.UtcNow;
            claim.Item.UpdatedAt = DateTime.UtcNow;
            
            // Reject all other pending claims for this item
            var otherClaims = await _context.Claims
                .Where(c => c.ItemId == claim.ItemId && c.Id != id && c.Status == ClaimStatus.Pending)
                .ToListAsync();
            
            foreach (var otherClaim in otherClaims)
            {
                otherClaim.Status = ClaimStatus.Rejected;
                otherClaim.AdminNotes = "Another claim was approved for this item.";
                otherClaim.ReviewedByUserId = adminUserId;
                otherClaim.DateReviewed = DateTime.UtcNow;
                otherClaim.UpdatedAt = DateTime.UtcNow;
                
                // Notify other claimants
                await _notificationService.CreateNotificationAsync(
                    otherClaim.ClaimantUserId,
                    "Claim Rejected",
                    $"Your claim for '{claim.Item.Title}' was not approved. Another claim was approved.",
                    NotificationType.ClaimUpdate,
                    claim.ItemId,
                    otherClaim.Id);
            }
        }
        
        await _context.SaveChangesAsync();
        
        // Notify the claimant
        var statusMessage = dto.Status switch
        {
            ClaimStatus.Approved => "approved! Please contact the campus lost and found office to collect your item.",
            ClaimStatus.Rejected => "not approved. Please see the admin notes for more details.",
            _ => "updated."
        };
        
        await _notificationService.CreateNotificationAsync(
            claim.ClaimantUserId,
            $"Claim {dto.Status}",
            $"Your claim for '{claim.Item.Title}' has been {statusMessage}",
            NotificationType.ClaimUpdate,
            claim.ItemId,
            claim.Id);
        
        return await GetClaimByIdAsync(id);
    }
    
    public async Task<bool> CancelClaimAsync(int id, string userId)
    {
        var claim = await _context.Claims.FindAsync(id);
        
        if (claim == null || claim.ClaimantUserId != userId || claim.Status != ClaimStatus.Pending)
            return false;
        
        claim.Status = ClaimStatus.Cancelled;
        claim.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    private static ClaimDto MapToDto(Claim claim)
    {
        return new ClaimDto
        {
            Id = claim.Id,
            ItemId = claim.ItemId,
            ItemTitle = claim.Item.Title,
            ClaimantUserId = claim.ClaimantUserId,
            ClaimantName = claim.Claimant.FullName,
            ClaimantEmail = claim.Claimant.Email!,
            Description = claim.Description,
            ProofDescription = claim.ProofDescription,
            Status = claim.Status,
            AdminNotes = claim.AdminNotes,
            ReviewedByName = claim.ReviewedBy?.FullName,
            DateReviewed = claim.DateReviewed,
            CreatedAt = claim.CreatedAt
        };
    }
}
