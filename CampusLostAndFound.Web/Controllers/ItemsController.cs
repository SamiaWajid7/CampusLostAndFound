using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CampusLostAndFound.Web.Models.DTOs;
using CampusLostAndFound.Web.Models.Enums;
using CampusLostAndFound.Web.Services;
using CampusLostAndFound.Web.Data;

namespace CampusLostAndFound.Web.Controllers;

public class ItemsController : Controller
{
    private readonly IItemService _itemService;
    private readonly ICategoryService _categoryService;
    private readonly ILocationService _locationService;
    private readonly IFileService _fileService;
    private readonly ApplicationDbContext _context;

    public ItemsController(
     IItemService itemService,
     ICategoryService categoryService,
     ILocationService locationService,
     IFileService fileService,
     ApplicationDbContext context)
    {
        _itemService = itemService;
        _categoryService = categoryService;
        _locationService = locationService;
        _fileService = fileService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ItemSearchDto search)
    {
        var items = await _itemService.GetItemsAsync(search);
        var categories = await _categoryService.GetAllCategoriesAsync();
        var locations = await _locationService.GetAllLocationsAsync();
        
        ViewData["Categories"] = categories;
        ViewData["Locations"] = locations;
        ViewData["Search"] = search;
        
        return View(items);
    }
    
    [HttpGet]
    public async Task<IActionResult> Lost([FromQuery] ItemSearchDto search)
    {
        search.ItemType = ItemType.Lost;
        var items = await _itemService.GetItemsAsync(search);
        var categories = await _categoryService.GetAllCategoriesAsync();
        var locations = await _locationService.GetAllLocationsAsync();
        
        ViewData["Categories"] = categories;
        ViewData["Locations"] = locations;
        ViewData["Search"] = search;
        ViewData["Title"] = "Lost Items";
        
        return View("Index", items);
    }
    
    [HttpGet]
    public async Task<IActionResult> Found([FromQuery] ItemSearchDto search)
    {
        search.ItemType = ItemType.Found;
        var items = await _itemService.GetItemsAsync(search);
        var categories = await _categoryService.GetAllCategoriesAsync();
        var locations = await _locationService.GetAllLocationsAsync();
        
        ViewData["Categories"] = categories;
        ViewData["Locations"] = locations;
        ViewData["Search"] = search;
        ViewData["Title"] = "Found Items";
        
        return View("Index", items);
    }
    
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var item = await _itemService.GetItemByIdAsync(id);
        
        if (item == null)
        {
            return NotFound();
        }
        
        // Increment view count
        await _itemService.IncrementViewCountAsync(id);
        
        return View(item);
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Create(ItemType? type = null)
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        var locations = await _locationService.GetAllLocationsAsync();
        
        ViewData["Categories"] = categories;
        ViewData["Locations"] = locations;
        ViewData["ItemType"] = type ?? ItemType.Lost;
        
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(CreateItemDto model, List<IFormFile>? images)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }
        
        if (!ModelState.IsValid)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var locations = await _locationService.GetAllLocationsAsync();
            ViewData["Categories"] = categories;
            ViewData["Locations"] = locations;
            return View(model);
        }

        var item = await _itemService.CreateItemAsync(model, userId);

        // Save images if provided
        if (images != null && images.Any())
        {
            var imageEntities = await _fileService.SaveImagesAsync(images, item.Id);

            _context.ItemImages.AddRange(imageEntities);
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = $"Your {model.ItemType.ToString().ToLower()} item has been reported successfully!";
        return RedirectToAction("Details", new { id = item.Id });
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var item = await _itemService.GetItemByIdAsync(id);
        
        if (item == null)
        {
            return NotFound();
        }
        
        if (item.ReportedByUserId != userId)
        {
            return Forbid();
        }
        
        var categories = await _categoryService.GetAllCategoriesAsync();
        var locations = await _locationService.GetAllLocationsAsync();
        
        ViewData["Categories"] = categories;
        ViewData["Locations"] = locations;
        
        return View(item);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int id, UpdateItemDto model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }
        
        if (!ModelState.IsValid)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var locations = await _locationService.GetAllLocationsAsync();
            ViewData["Categories"] = categories;
            ViewData["Locations"] = locations;
            return View(model);
        }
        
        var result = await _itemService.UpdateItemAsync(id, model, userId);
        
        if (result == null)
        {
            return NotFound();
        }
        
        TempData["SuccessMessage"] = "Item updated successfully!";
        return RedirectToAction("Details", new { id });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }
        
        var result = await _itemService.DeleteItemAsync(id, userId);
        
        if (!result)
        {
            return NotFound();
        }
        
        TempData["SuccessMessage"] = "Item deleted successfully!";
        return RedirectToAction("Index", "Dashboard");
    }
    
    [HttpGet]
    public async Task<IActionResult> Search(string q)
    {
        var search = new ItemSearchDto
        {
            SearchTerm = q,
            PageSize = 20
        };
        
        var items = await _itemService.GetItemsAsync(search);
        var categories = await _categoryService.GetAllCategoriesAsync();
        var locations = await _locationService.GetAllLocationsAsync();
        
        ViewData["Categories"] = categories;
        ViewData["Locations"] = locations;
        ViewData["Search"] = search;
        ViewData["SearchTerm"] = q;
        
        return View("Index", items);
    }
}
