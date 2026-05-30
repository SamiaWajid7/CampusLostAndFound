using Microsoft.EntityFrameworkCore;
using CampusLostAndFound.Web.Data;
using CampusLostAndFound.Web.Models.DTOs;
using CampusLostAndFound.Web.Models.Entities;
using CampusLostAndFound.Web.Models.Enums;

namespace CampusLostAndFound.Web.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int count = 20);
    Task<int> GetUnreadCountAsync(string userId);
    Task CreateNotificationAsync(string userId, string title, string message, NotificationType type, int? itemId = null, int? claimId = null);
    Task MarkAsReadAsync(int id, string userId);
    Task MarkAllAsReadAsync(string userId);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    
    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int count = 20)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                RelatedItemId = n.RelatedItemId,
                RelatedClaimId = n.RelatedClaimId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();
    }
    
    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }
    
    public async Task CreateNotificationAsync(
        string userId, 
        string title, 
        string message, 
        NotificationType type, 
        int? itemId = null, 
        int? claimId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            RelatedItemId = itemId,
            RelatedClaimId = claimId
        };
        
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }
    
    public async Task MarkAsReadAsync(int id, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        
        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task MarkAllAsReadAsync(string userId)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();
        
        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
    }
}
