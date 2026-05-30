using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CampusLostAndFound.Web.Models.DTOs;
using CampusLostAndFound.Web.Services;

namespace CampusLostAndFound.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    
    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var (success, message, user) = await _authService.LoginAsync(model);
        
        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToLocal(returnUrl);
        }
        
        ModelState.AddModelError(string.Empty, message);
        return View(model);
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
            return View(model);
        }
        
        var (success, message, user) = await _authService.RegisterAsync(model);
        
        if (success)
        {
            TempData["SuccessMessage"] = "Registration successful! Welcome to Campus Lost and Found.";
            return RedirectToAction("Index", "Dashboard");
        }
        
        ModelState.AddModelError(string.Empty, message);
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login");
        }
        
        var user = await _authService.GetCurrentUserAsync(userId);
        return View(user);
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
    {
        if (newPassword != confirmNewPassword)
        {
            ModelState.AddModelError("confirmNewPassword", "Passwords do not match.");
            return View();
        }
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login");
        }
        
        var (success, message) = await _authService.ChangePasswordAsync(userId, currentPassword, newPassword);
        
        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction("Profile");
        }
        
        ModelState.AddModelError(string.Empty, message);
        return View();
    }
    
    public IActionResult AccessDenied()
    {
        return View();
    }
    
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        
        return RedirectToAction("Index", "Dashboard");
    }
}
