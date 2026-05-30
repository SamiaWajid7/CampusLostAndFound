using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using CampusLostAndFound.Web.Models.Entities;
using CampusLostAndFound.Web.Data;

namespace CampusLostAndFound.Web.Services;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Apply migrations
        await context.Database.MigrateAsync();

        // Try seeding. If Identity tables are missing despite migrations (rare), create DB schema and retry.
        try
        {
            // Seed roles
            await SeedRolesAsync(roleManager);

            // Seed admin user
            await SeedAdminUserAsync(userManager);

            // Seed categories
            await SeedCategoriesAsync(context);

            // Seed locations
            await SeedLocationsAsync(context);
        }
        catch (SqlException ex) when (ex.Number == 208) // Invalid object name
        {
            // If the server/database is unreachable, surface the error so it can be fixed
            if (!await context.Database.CanConnectAsync())
            {
                throw;
            }

            // Prefer applying migrations when using EF Migrations instead of EnsureCreated
            await context.Database.MigrateAsync();

            // Retry seeding
            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager);
            await SeedCategoriesAsync(context);
            await SeedLocationsAsync(context);
        }
    }
    
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "Student" };
        
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
    
    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@campus.edu";
        const string adminPassword = "Admin@123!";
        
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true
            };
            
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
    
    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;
        
        var categories = new List<Category>
        {
            new() { Name = "Electronics", Description = "Phones, laptops, tablets, chargers, headphones, etc.", IconClass = "bi-phone" },
            new() { Name = "Bags & Wallets", Description = "Backpacks, purses, wallets, laptop bags, etc.", IconClass = "bi-bag" },
            new() { Name = "Keys", Description = "Car keys, room keys, key cards, key chains, etc.", IconClass = "bi-key" },
            new() { Name = "ID & Cards", Description = "Student IDs, credit cards, driver licenses, etc.", IconClass = "bi-credit-card" },
            new() { Name = "Clothing", Description = "Jackets, hats, scarves, gloves, shoes, etc.", IconClass = "bi-person-badge" },
            new() { Name = "Books & Notes", Description = "Textbooks, notebooks, planners, documents, etc.", IconClass = "bi-book" },
            new() { Name = "Accessories", Description = "Jewelry, watches, glasses, umbrellas, etc.", IconClass = "bi-gem" },
            new() { Name = "Sports Equipment", Description = "Balls, rackets, water bottles, gym gear, etc.", IconClass = "bi-dribbble" },
            new() { Name = "Food Containers", Description = "Lunch boxes, water bottles, thermoses, etc.", IconClass = "bi-cup-straw" },
            new() { Name = "Other", Description = "Items that do not fit other categories", IconClass = "bi-question-circle" }
        };
        
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedLocationsAsync(ApplicationDbContext context)
    {
        if (await context.Locations.AnyAsync()) return;
        
        var locations = new List<Location>
        {
            new() { Name = "Main Library", Building = "Library Building", Description = "Central campus library with study areas and computer labs" },
            new() { Name = "Science Building - Floor 1", Building = "Science Building", Floor = "1st Floor", Description = "Chemistry and Physics labs" },
            new() { Name = "Science Building - Floor 2", Building = "Science Building", Floor = "2nd Floor", Description = "Biology labs and lecture halls" },
            new() { Name = "Student Center", Building = "Student Center", Description = "Cafeteria, lounges, and student services" },
            new() { Name = "Engineering Hall", Building = "Engineering Building", Description = "Engineering classrooms and workshops" },
            new() { Name = "Arts Building", Building = "Arts Building", Description = "Art studios, music rooms, and theater" },
            new() { Name = "Business School", Building = "Business Building", Description = "Business and economics classrooms" },
            new() { Name = "Sports Complex", Building = "Athletic Center", Description = "Gym, pool, and sports facilities" },
            new() { Name = "Dormitory A", Building = "Residence Hall A", Description = "Student housing - North Campus" },
            new() { Name = "Dormitory B", Building = "Residence Hall B", Description = "Student housing - South Campus" },
            new() { Name = "Cafeteria", Building = "Dining Hall", Description = "Main campus dining facility" },
            new() { Name = "Parking Lot A", Building = "Outdoor", Description = "Main student parking area" },
            new() { Name = "Parking Lot B", Building = "Outdoor", Description = "Staff and visitor parking" },
            new() { Name = "Campus Bus Stop", Building = "Outdoor", Description = "Main bus stop near entrance" },
            new() { Name = "Admin Building", Building = "Administration", Description = "Registrar, financial aid, and admin offices" }
        };
        
        context.Locations.AddRange(locations);
        await context.SaveChangesAsync();
    }
}
