using Microsoft.EntityFrameworkCore;
using CampusLostAndFound.Web.Data;
using CampusLostAndFound.Web.Models.DTOs;
using CampusLostAndFound.Web.Models.Entities;
using CampusLostAndFound.Web.Models.Enums;

namespace CampusLostAndFound.Web.Services;

public interface IItemService
{
    Task<PagedResultDto<ItemListDto>> GetItemsAsync(ItemSearchDto search);
    Task<ItemDto?> GetItemByIdAsync(int id);
    Task<List<ItemListDto>> GetRecentItemsAsync(int count = 10);
    Task<List<ItemListDto>> GetUserItemsAsync(string userId);
    Task<ItemDto> CreateItemAsync(CreateItemDto dto, string userId);
    Task<ItemDto?> UpdateItemAsync(int id, UpdateItemDto dto, string userId);
    Task<bool> DeleteItemAsync(int id, string userId);
    Task<bool> VerifyItemAsync(int id, string adminUserId, string? notes);
    Task<bool> MarkAsReturnedAsync(int id, string adminUserId);
    Task IncrementViewCountAsync(int id);
}

public class ItemService : IItemService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    
    public ItemService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }
    
    public async Task<PagedResultDto<ItemListDto>> GetItemsAsync(ItemSearchDto search)
    {
        var query = _context.Items
            .Include(i => i.Category)
            .Include(i => i.Location)
            .Include(i => i.Images)
            .AsQueryable();
        
        // Apply filters
        if (!string.IsNullOrWhiteSpace(search.SearchTerm))
        {
            var term = search.SearchTerm.ToLower();
            query = query.Where(i => 
                i.Title.ToLower().Contains(term) || 
                i.Description.ToLower().Contains(term));
        }
        
        if (search.CategoryId.HasValue)
            query = query.Where(i => i.CategoryId == search.CategoryId.Value);
            
        if (search.LocationId.HasValue)
            query = query.Where(i => i.LocationId == search.LocationId.Value);
            
        if (search.ItemType.HasValue)
            query = query.Where(i => i.ItemType == search.ItemType.Value);
            
        if (search.Status.HasValue)
            query = query.Where(i => i.Status == search.Status.Value);
            
        if (search.DateFrom.HasValue)
            query = query.Where(i => i.DateOccurred >= search.DateFrom.Value);
            
        if (search.DateTo.HasValue)
            query = query.Where(i => i.DateOccurred <= search.DateTo.Value);
        
        // Get total count
        var totalCount = await query.CountAsync();
        
        // Apply sorting
        query = search.SortBy.ToLower() switch
        {
            "title" => search.SortDescending ? query.OrderByDescending(i => i.Title) : query.OrderBy(i => i.Title),
            "dateoccurred" => search.SortDescending ? query.OrderByDescending(i => i.DateOccurred) : query.OrderBy(i => i.DateOccurred),
            _ => search.SortDescending ? query.OrderByDescending(i => i.CreatedAt) : query.OrderBy(i => i.CreatedAt)
        };
        
        // Apply pagination
        var items = await query
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
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
        
        return new PagedResultDto<ItemListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = search.Page,
            PageSize = search.PageSize
        };
    }
    
    public async Task<ItemDto?> GetItemByIdAsync(int id)
    {
        var item = await _context.Items
            .Include(i => i.Category)
            .Include(i => i.Location)
            .Include(i => i.ReportedBy)
            .Include(i => i.Images)
            .Include(i => i.Claims)
            .FirstOrDefaultAsync(i => i.Id == id);
        
        if (item == null) return null;
        
        return new ItemDto
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            CategoryId = item.CategoryId,
            CategoryName = item.Category.Name,
            CategoryIcon = item.Category.IconClass,
            LocationId = item.LocationId,
            LocationName = item.Location.Name,
            LocationDetails = item.LocationDetails,
            ItemType = item.ItemType,
            Status = item.Status,
            DateOccurred = item.DateOccurred,
            ReportedByUserId = item.ReportedByUserId,
            ReportedByName = item.ReportedBy.FullName,
            IsVerified = item.IsVerified,
            ViewCount = item.ViewCount,
            ClaimCount = item.Claims.Count,
            CreatedAt = item.CreatedAt,
            Images = item.Images.Select(img => new ItemImageDto
            {
                Id = img.Id,
                FileName = img.FileName,
                FilePath = img.FilePath,
                IsPrimary = img.IsPrimary
            }).ToList()
        };
    }
    
    public async Task<List<ItemListDto>> GetRecentItemsAsync(int count = 10)
    {
        return await _context.Items
            .Include(i => i.Category)
            .Include(i => i.Location)
            .Include(i => i.Images)
            .Where(i => i.Status == ItemStatus.Open)
            .OrderByDescending(i => i.CreatedAt)
            .Take(count)
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
    }
    
    public async Task<List<ItemListDto>> GetUserItemsAsync(string userId)
    {
        return await _context.Items
            .Include(i => i.Category)
            .Include(i => i.Location)
            .Include(i => i.Images)
            .Where(i => i.ReportedByUserId == userId)
            .OrderByDescending(i => i.CreatedAt)
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
    }
    
    public async Task<ItemDto> CreateItemAsync(CreateItemDto dto, string userId)
    {
        var item = new Item
        {
            Title = dto.Title,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            LocationId = dto.LocationId,
            LocationDetails = dto.LocationDetails,
            ItemType = dto.ItemType,
            DateOccurred = dto.DateOccurred,
            ReportedByUserId = userId,
            Status = ItemStatus.Open
        };
        
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        
        return (await GetItemByIdAsync(item.Id))!;
    }
    
    public async Task<ItemDto?> UpdateItemAsync(int id, UpdateItemDto dto, string userId)
    {
        var item = await _context.Items.FindAsync(id);
        
        if (item == null || item.ReportedByUserId != userId)
            return null;
        
        item.Title = dto.Title;
        item.Description = dto.Description;
        item.CategoryId = dto.CategoryId;
        item.LocationId = dto.LocationId;
        item.LocationDetails = dto.LocationDetails;
        item.DateOccurred = dto.DateOccurred;
        item.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return await GetItemByIdAsync(id);
    }
    
    public async Task<bool> DeleteItemAsync(int id, string userId)
    {
        var item = await _context.Items.FindAsync(id);
        
        if (item == null || item.ReportedByUserId != userId)
            return false;
        
        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<bool> VerifyItemAsync(int id, string adminUserId, string? notes)
    {
        var item = await _context.Items.FindAsync(id);
        
        if (item == null) return false;
        
        item.IsVerified = true;
        item.VerifiedByUserId = adminUserId;
        item.VerificationNotes = notes;
        item.DateVerified = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        // Notify the reporter
        await _notificationService.CreateNotificationAsync(
            item.ReportedByUserId,
            "Item Verified",
            $"Your item '{item.Title}' has been verified by an administrator.",
            NotificationType.Success,
            item.Id);
        
        return true;
    }
    
    public async Task<bool> MarkAsReturnedAsync(int id, string adminUserId)
    {
        var item = await _context.Items.FindAsync(id);
        
        if (item == null) return false;
        
        item.Status = ItemStatus.Returned;
        item.DateReturned = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        // Notify the reporter
        await _notificationService.CreateNotificationAsync(
            item.ReportedByUserId,
            "Item Returned",
            $"Your item '{item.Title}' has been marked as returned.",
            NotificationType.Success,
            item.Id);
        
        return true;
    }
    
    public async Task IncrementViewCountAsync(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item != null)
        {
            item.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }
}
