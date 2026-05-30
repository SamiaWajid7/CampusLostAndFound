using Microsoft.AspNetCore.Mvc;
using CampusLostAndFound.Web.Services;

namespace CampusLostAndFound.Web.Controllers;

public class HomeController : Controller
{
    private readonly IItemService _itemService;
    private readonly ICategoryService _categoryService;
    
    public HomeController(IItemService itemService, ICategoryService categoryService)
    {
        _itemService = itemService;
        _categoryService = categoryService;
    }
    
    public async Task<IActionResult> Index()
    {
        var recentItems = await _itemService.GetRecentItemsAsync(8);
        var categories = await _categoryService.GetAllCategoriesAsync();
        
        ViewData["Categories"] = categories;
        return View(recentItems);
    }
    
    public IActionResult About()
    {
        return View();
    }
    
    public IActionResult Contact()
    {
        return View();
    }
    
    public IActionResult Privacy()
    {
        return View();
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
