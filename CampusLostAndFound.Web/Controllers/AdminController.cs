using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CampusLostAndFound.Web.Models.DTOs;
using CampusLostAndFound.Web.Services;

namespace CampusLostAndFound.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("Admin")]
public class AdminController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IItemService _itemService;
    private readonly IClaimService _claimService;
    
    public AdminController(
        IDashboardService dashboardService,
        IItemService itemService,
        IClaimService claimService)
    {
        _dashboardService = dashboardService;
        _itemService = itemService;
        _claimService = claimService;
    }
    
    [HttpGet]
    [Route("")]
    [Route("Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var stats = await _dashboardService.GetAdminDashboardStatsAsync();
        return View(stats);
    }
    
    [HttpGet]
    [Route("Items")]
    public async Task<IActionResult> Items([FromQuery] ItemSearchDto search)
    {
        var items = await _itemService.GetItemsAsync(search);
        ViewData["Search"] = search;
        return View(items);
    }
    
    [HttpGet]
    [Route("Claims")]
    public async Task<IActionResult> Claims()
    {
        var claims = await _claimService.GetPendingClaimsAsync();
        return View(claims);
    }
    
    [HttpGet]
    [Route("Claims/{id:int}")]
    public async Task<IActionResult> ReviewClaim(int id)
    {
        var claim = await _claimService.GetClaimByIdAsync(id);
        
        if (claim == null)
        {
            return NotFound();
        }
        
        var item = await _itemService.GetItemByIdAsync(claim.ItemId);
        ViewData["Item"] = item;
        
        // Get all claims for this item
        var allClaims = await _claimService.GetClaimsForItemAsync(claim.ItemId);
        ViewData["AllClaims"] = allClaims;
        
        return View(claim);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Claims/{id:int}/Review")]
    public async Task<IActionResult> ReviewClaim(int id, ReviewClaimDto model)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminUserId))
        {
            return RedirectToAction("Login", "Account");
        }
        
        var result = await _claimService.ReviewClaimAsync(id, model, adminUserId);
        
        if (result == null)
        {
            return NotFound();
        }
        
        TempData["SuccessMessage"] = $"Claim has been {model.Status.ToString().ToLower()}.";
        return RedirectToAction("Claims");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Items/{id:int}/Verify")]
    public async Task<IActionResult> VerifyItem(int id, string? notes)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminUserId))
        {
            return RedirectToAction("Login", "Account");
        }
        
        var result = await _itemService.VerifyItemAsync(id, adminUserId, notes);
        
        if (!result)
        {
            return NotFound();
        }
        
        TempData["SuccessMessage"] = "Item has been verified.";
        return RedirectToAction("Items");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Items/{id:int}/MarkReturned")]
    public async Task<IActionResult> MarkItemReturned(int id)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminUserId))
        {
            return RedirectToAction("Login", "Account");
        }
        
        var result = await _itemService.MarkAsReturnedAsync(id, adminUserId);
        
        if (!result)
        {
            return NotFound();
        }
        
        TempData["SuccessMessage"] = "Item has been marked as returned.";
        return RedirectToAction("Items");
    }
}
