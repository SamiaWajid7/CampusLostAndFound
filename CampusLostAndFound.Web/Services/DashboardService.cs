using Microsoft.EntityFrameworkCore;
using CampusLostAndFound.Web.Data;
using CampusLostAndFound.Web.Models.DTOs;
using CampusLostAndFound.Web.Models.Enums;

namespace CampusLostAndFound.Web.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetAdminDashboardStatsAsync();
    Task<UserDashboardDto> GetUserDashboardAsync(string userId);
}

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    
    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<DashboardStatsDto> GetAdminDashboardStatsAsync()
    {
        var stats = new DashboardStatsDto
        {
            TotalLostItems = await _context.Items.CountAsync(i => i.ItemType == ItemType.Lost),
            TotalFoundItems = await _context.Items.CountAsync(i => i.ItemType == ItemType.Found),
            OpenItems = await _context.Items.CountAsync(i => i.Status == ItemStatus.Open),
            ReturnedItems = await _context.Items.CountAsync(i => i.Status == ItemStatus.Returned),
            PendingClaims = await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Pending),
            TotalUsers = await _context.Users.CountAsync()
        };
        
        // Category statistics
        stats.CategoryStats = await _context.Categories
            .Select(c => new CategoryStatsDto
            {
                CategoryName = c.Name,
                LostCount = c.Items.Count(i => i.ItemType == ItemType.Lost),
                FoundCount = c.Items.Count(i => i.ItemType == ItemType.Found)
            })
            .ToListAsync();
        
        // Recent activity
        var recentItems = await _context.Items
            .Include(i => i.ReportedBy)
            .OrderByDescending(i => i.CreatedAt)
            .Take(10)
            .Select(i => new RecentActivityDto
            {
                Action = i.ItemType == ItemType.Lost ? "Reported Lost" : "Reported Found",
                ItemTitle = i.Title,
                ItemId = i.Id,
                UserName = i.ReportedBy.FirstName + " " + i.ReportedBy.LastName,
                Timestamp = i.CreatedAt
            })
            .ToListAsync();
        
        var recentClaims = await _context.Claims
            .Include(c => c.Item)
            .Include(c => c.Claimant)
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new RecentActivityDto
            {
                Action = "Submitted Claim",
                ItemTitle = c.Item.Title,
                ItemId = c.ItemId,
                UserName = c.Claimant.FirstName + " " + c.Claimant.LastName,
                Timestamp = c.CreatedAt
            })
            .ToListAsync();
        
        stats.RecentActivity = recentItems
            .Concat(recentClaims)
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .ToList();
        
        return stats;
    }
    
    public async Task<UserDashboardDto> GetUserDashboardAsync(string userId)
    {
        var dashboard = new UserDashboardDto
        {
            MyLostItems = await _context.Items.CountAsync(i => i.ReportedByUserId == userId && i.ItemType == ItemType.Lost),
            MyFoundItems = await _context.Items.CountAsync(i => i.ReportedByUserId == userId && i.ItemType == ItemType.Found),
            MyClaims = await _context.Claims.CountAsync(c => c.ClaimantUserId == userId),
            UnreadNotifications = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead)
        };
        
        // Recent items by user
        dashboard.RecentItems = await _context.Items
            .Include(i => i.Category)
            .Include(i => i.Location)
            .Include(i => i.Images)
            .Where(i => i.ReportedByUserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .Take(5)
            .Select(i => new ItemListDto
            {
                Id = i.Id,
                Title = i.Title,
                CategoryName = i.Category.Name,
                CategoryIcon = i.Category.IconClass,
                LocationName = i.Location.Name,
                ItemType = i.ItemType,
                Status = i.Status,
                DateOccurred = i.DateOccurred,
                PrimaryImagePath = i.Images.Where(img => img.IsPrimary).Select(img => img.FilePath).FirstOrDefault()
                    ?? i.Images.Select(img => img.FilePath).FirstOrDefault(),
                IsVerified = i.IsVerified,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();
        
        // Recent claims by user
        dashboard.RecentClaims = await _context.Claims
            .Include(c => c.Item)
            .Include(c => c.Claimant)
            .Include(c => c.ReviewedBy)
            .Where(c => c.ClaimantUserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new ClaimDto
            {
                Id = c.Id,
                ItemId = c.ItemId,
                ItemTitle = c.Item.Title,
                ClaimantUserId = c.ClaimantUserId,
                ClaimantName = c.Claimant.FirstName + " " + c.Claimant.LastName,
                ClaimantEmail = c.Claimant.Email!,
                Description = c.Description,
                ProofDescription = c.ProofDescription,
                Status = c.Status,
                AdminNotes = c.AdminNotes,
                ReviewedByName = c.ReviewedBy != null ? c.ReviewedBy.FirstName + " " + c.ReviewedBy.LastName : null,
                DateReviewed = c.DateReviewed,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
        
        return dashboard;
    }
}
