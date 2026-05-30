using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CampusLostAndFound.Web.Services;

namespace CampusLostAndFound.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly INotificationService _notificationService;
    private readonly IAuthService _authService;
    
    public DashboardController(
        IDashboardService dashboardService,
        INotificationService notificationService,
        IAuthService authService)
    {
        _dashboardService = dashboardService;
        _notificationService = notificationService;
        _authService = authService;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }
        
        var dashboard = await _dashboardService.GetUserDashboardAsync(userId);
        return View(dashboard);
    }
    
    [HttpGet]
    public async Task<IActionResult> Notifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }
        
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, 50);
        return View(notifications);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNotificationRead(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false });
        }
        
        await _notificationService.MarkAsReadAsync(id, userId);
        return Json(new { success = true });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllNotificationsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }
        
        await _notificationService.MarkAllAsReadAsync(userId);
        TempData["SuccessMessage"] = "All notifications marked as read.";
        return RedirectToAction("Notifications");
    }
    
    [HttpGet]
    public async Task<IActionResult> GetNotificationCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { count = 0 });
        }
        
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Json(new { count });
    }
}
