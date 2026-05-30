using Microsoft.EntityFrameworkCore;
using CampusLostAndFound.Web.Data;
using CampusLostAndFound.Web.Models.DTOs;

namespace CampusLostAndFound.Web.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
}

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    
    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IconClass = c.IconClass,
                ItemCount = c.Items.Count
            })
            .ToListAsync();
    }
    
    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);
        
        if (category == null) return null;
        
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IconClass = category.IconClass,
            ItemCount = category.Items.Count
        };
    }
}

public interface ILocationService
{
    Task<List<LocationDto>> GetAllLocationsAsync();
    Task<LocationDto?> GetLocationByIdAsync(int id);
}

public class LocationService : ILocationService
{
    private readonly ApplicationDbContext _context;
    
    public LocationService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<LocationDto>> GetAllLocationsAsync()
    {
        return await _context.Locations
            .Where(l => l.IsActive)
            .OrderBy(l => l.Building)
            .ThenBy(l => l.Name)
            .Select(l => new LocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Building = l.Building,
                Floor = l.Floor,
                Description = l.Description,
                ItemCount = l.Items.Count
            })
            .ToListAsync();
    }
    
    public async Task<LocationDto?> GetLocationByIdAsync(int id)
    {
        var location = await _context.Locations
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == id);
        
        if (location == null) return null;
        
        return new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Building = location.Building,
            Floor = location.Floor,
            Description = location.Description,
            ItemCount = location.Items.Count
        };
    }
}
