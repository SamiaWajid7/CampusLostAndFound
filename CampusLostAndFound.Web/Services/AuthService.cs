using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CampusLostAndFound.Web.Models.Entities;
using CampusLostAndFound.Web.Models.DTOs;

namespace CampusLostAndFound.Web.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, UserDto? User)> RegisterAsync(RegisterDto model);
    Task<(bool Success, string Message, UserDto? User)> LoginAsync(LoginDto model);
    Task LogoutAsync();
    Task<UserDto?> GetCurrentUserAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<(bool Success, string Message)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    
    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }
    
    public async Task<(bool Success, string Message, UserDto? User)> RegisterAsync(RegisterDto model)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            return (false, "A user with this email already exists.", null);
        }
        
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            StudentId = model.StudentId,
            Department = model.Department,
            EmailConfirmed = true // For simplicity; in production, implement email confirmation
        };
        
        var result = await _userManager.CreateAsync(user, model.Password);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, errors, null);
        }
        
        // Ensure Student role exists and assign it
        if (!await _roleManager.RoleExistsAsync("Student"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Student"));
        }
        
        await _userManager.AddToRoleAsync(user, "Student");
        
        // Auto sign-in after registration
        await _signInManager.SignInAsync(user, isPersistent: false);
        
        var userDto = await MapToUserDto(user);
        return (true, "Registration successful!", userDto);
    }
    
    public async Task<(bool Success, string Message, UserDto? User)> LoginAsync(LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        
        if (user == null)
        {
            return (false, "Invalid email or password.", null);
        }
        
        if (!user.IsActive)
        {
            return (false, "Your account has been deactivated. Please contact support.", null);
        }
        
        var result = await _signInManager.PasswordSignInAsync(
            user, 
            model.Password, 
            model.RememberMe, 
            lockoutOnFailure: true);
        
        if (result.Succeeded)
        {
            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            
            var userDto = await MapToUserDto(user);
            return (true, "Login successful!", userDto);
        }
        
        if (result.IsLockedOut)
        {
            return (false, "Account is locked. Please try again later.", null);
        }
        
        return (false, "Invalid email or password.", null);
    }
    
    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
    
    public async Task<UserDto?> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;
        
        return await MapToUserDto(user);
    }
    
    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        
        return await _userManager.IsInRoleAsync(user, role);
    }
    
    public async Task<(bool Success, string Message)> ChangePasswordAsync(
        string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found.");
        }
        
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        
        if (result.Succeeded)
        {
            return (true, "Password changed successfully.");
        }
        
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return (false, errors);
    }
    
    private async Task<UserDto> MapToUserDto(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            StudentId = user.StudentId,
            Department = user.Department,
            ProfileImagePath = user.ProfileImagePath,
            Roles = roles,
            CreatedAt = user.CreatedAt
        };
    }
}
