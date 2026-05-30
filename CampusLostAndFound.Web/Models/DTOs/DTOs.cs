using CampusLostAndFound.Web.Models.Enums;

namespace CampusLostAndFound.Web.Models.DTOs;

// ========================================
// Authentication DTOs
// ========================================

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? StudentId { get; set; }
    public string? Department { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? StudentId { get; set; }
    public string? Department { get; set; }
    public string? ProfileImagePath { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
}

// ========================================
// Item DTOs
// ========================================

public class CreateItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int LocationId { get; set; }
    public string? LocationDetails { get; set; }
    public ItemType ItemType { get; set; }
    public DateTime DateOccurred { get; set; }
}

public class UpdateItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int LocationId { get; set; }
    public string? LocationDetails { get; set; }
    public DateTime DateOccurred { get; set; }
}

public class ItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string? LocationDetails { get; set; }
    public ItemType ItemType { get; set; }
    public string ItemTypeName => ItemType.ToString();
    public ItemStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime DateOccurred { get; set; }
    public string ReportedByUserId { get; set; } = string.Empty;
    public string ReportedByName { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public int ViewCount { get; set; }
    public int ClaimCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ItemImageDto> Images { get; set; } = new();
}

public class ItemListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public ItemType ItemType { get; set; }
    public ItemStatus Status { get; set; }
    public DateTime DateOccurred { get; set; }
    public string? PrimaryImagePath { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}
public class ItemListViewDto
{
    public List<ItemListDto> Items { get; set; } = new();
}

public class ItemImageDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

// ========================================
// Claim DTOs
// ========================================

public class CreateClaimDto
{
    public int ItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ProofDescription { get; set; }
    public string? ContactInfo { get; set; }
}

public class ReviewClaimDto
{
    public ClaimStatus Status { get; set; }
    public string? AdminNotes { get; set; }
}

public class ClaimDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public string ClaimantUserId { get; set; } = string.Empty;
    public string ClaimantName { get; set; } = string.Empty;
    public string ClaimantEmail { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ProofDescription { get; set; }
    public ClaimStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? AdminNotes { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? DateReviewed { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ========================================
// Notification DTOs
// ========================================

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public int? RelatedItemId { get; set; }
    public int? RelatedClaimId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ========================================
// Category and Location DTOs
// ========================================

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconClass { get; set; }
    public int ItemCount { get; set; }
}

public class LocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Building { get; set; }
    public string? Floor { get; set; }
    public string? Description { get; set; }
    public int ItemCount { get; set; }
}

// ========================================
// Search and Filter DTOs
// ========================================

public class ItemSearchDto
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public int? LocationId { get; set; }
    public ItemType? ItemType { get; set; }
    public ItemStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

// ========================================
// Dashboard DTOs
// ========================================

public class DashboardStatsDto
{
    public int TotalLostItems { get; set; }
    public int TotalFoundItems { get; set; }
    public int OpenItems { get; set; }
    public int ReturnedItems { get; set; }
    public int PendingClaims { get; set; }
    public int TotalUsers { get; set; }
    public List<CategoryStatsDto> CategoryStats { get; set; } = new();
    public List<RecentActivityDto> RecentActivity { get; set; } = new();
}

public class CategoryStatsDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int LostCount { get; set; }
    public int FoundCount { get; set; }
}

public class RecentActivityDto
{
    public string Action { get; set; } = string.Empty;
    public string ItemTitle { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class UserDashboardDto
{
    public int MyLostItems { get; set; }
    public int MyFoundItems { get; set; }
    public int MyClaims { get; set; }
    public int UnreadNotifications { get; set; }
    public List<ItemListDto> RecentItems { get; set; } = new();
    public List<ClaimDto> RecentClaims { get; set; } = new();
}

// ========================================
// Additional Claim DTOs
// ========================================

public class ClaimListDto
{
    public List<ClaimDto> Claims { get; set; } = new();
}

public class ClaimDetailsDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public ItemType ItemType { get; set; }
    public string? ItemImageUrl { get; set; }
    public string? ItemLocation { get; set; }
    public string ClaimantUserId { get; set; } = string.Empty;
    public string ClaimantName { get; set; } = string.Empty;
    public string ClaimantEmail { get; set; } = string.Empty;
    public string? ClaimantStudentId { get; set; }
    public string? ClaimantDepartment { get; set; }
    public string? ContactInfo { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ProofDescription { get; set; }
    public ClaimStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? AdminNotes { get; set; }
    public string? ReviewedByUserId { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? DateReviewed { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public ItemDto? Item { get; set; }
}

// ========================================
// Audit Log DTOs
// ========================================

public class AuditLogListDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
