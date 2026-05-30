using CampusLostAndFound.Web.Models.DTOs;
using CampusLostAndFound.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace CampusLostAndFound.Web.Controllers;

[Authorize]
public class ClaimsController : Controller
{
    private readonly IClaimService _claimService;
    private readonly IItemService _itemService;
    
    public ClaimsController(IClaimService claimService, IItemService itemService)
    {
        _claimService = claimService;
        _itemService = itemService;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }
        
        var claims = await _claimService.GetUserClaimsAsync(userId);
        return View(claims);
    }
    
    [HttpGet]
    public async Task<IActionResult> Create(int itemId)
    {
        var item = await _itemService.GetItemByIdAsync(itemId);
        
        if (item == null)
        {
            return NotFound();
        }
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (item.ReportedByUserId == userId)
        {
            TempData["ErrorMessage"] = "You cannot claim your own item.";
            return RedirectToAction("Details", "Items", new { id = itemId });
        }
        
        ViewData["Item"] = item;
        return View(new CreateClaimDto { ItemId = itemId });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateClaimDto model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }
        
        if (!ModelState.IsValid)
        {
            var item = await _itemService.GetItemByIdAsync(model.ItemId);
            ViewData["Item"] = item;
            return View(model);
        }
        
        try
        {
            await _claimService.CreateClaimAsync(model, userId);
            TempData["SuccessMessage"] = "Your claim has been submitted successfully. An administrator will review it shortly.";
            return RedirectToAction("Index");
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Details", "Items", new { id = model.ItemId });
        }
    }
    
    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var claim = await _claimService.GetClaimByIdAsync(id);

        if (claim == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        // Get item first
        var item = await _itemService.GetItemByIdAsync(claim.ItemId);

        // Check if current user reported the item
        var isOwner = item?.ReportedByUserId == userId;

        // Allow claimant, item owner, or admin
        if (claim.ClaimantUserId != userId && !isOwner && !isAdmin)
        {
            return Forbid();
        }

        ViewData["Item"] = item;

        return View(claim);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }
        
        var result = await _claimService.CancelClaimAsync(id, userId);
        
        if (!result)
        {
            TempData["ErrorMessage"] = "Unable to cancel the claim. It may have already been reviewed.";
        }
        else
        {
            TempData["SuccessMessage"] = "Your claim has been cancelled.";
        }
        
        return RedirectToAction("Index");
    }
}
