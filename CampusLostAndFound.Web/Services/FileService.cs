using CampusLostAndFound.Web.Models.Entities;

namespace CampusLostAndFound.Web.Services;

public interface IFileService
{
    Task<ItemImage> SaveImageAsync(IFormFile file, int itemId);
    Task<List<ItemImage>> SaveImagesAsync(IEnumerable<IFormFile> files, int itemId);
    Task<bool> DeleteImageAsync(int imageId);
    Task<bool> SetPrimaryImageAsync(int itemId, int imageId);
}

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _uploadsPath;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    
    public FileService(IWebHostEnvironment environment)
    {
        _environment = environment;
        _uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "items");
        
        if (!Directory.Exists(_uploadsPath))
        {
            Directory.CreateDirectory(_uploadsPath);
        }
    }
    
    public async Task<ItemImage> SaveImageAsync(IFormFile file, int itemId)
    {
        ValidateFile(file);
        
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var itemFolder = Path.Combine(_uploadsPath, itemId.ToString());
        
        if (!Directory.Exists(itemFolder))
        {
            Directory.CreateDirectory(itemFolder);
        }
        
        var filePath = Path.Combine(itemFolder, fileName);
        var relativePath = $"/uploads/items/{itemId}/{fileName}";
        
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        return new ItemImage
        {
            ItemId = itemId,
            FileName = file.FileName,
            FilePath = relativePath,
            FileSize = file.Length,
            ContentType = file.ContentType,
            IsPrimary = false
        };
    }
    
    public async Task<List<ItemImage>> SaveImagesAsync(IEnumerable<IFormFile> files, int itemId)
    {
        var images = new List<ItemImage>();
        var isFirst = true;
        
        foreach (var file in files)
        {
            var image = await SaveImageAsync(file, itemId);
            
            // First image is primary by default
            if (isFirst)
            {
                image.IsPrimary = true;
                isFirst = false;
            }
            
            images.Add(image);
        }
        
        return images;
    }
    
    public Task<bool> DeleteImageAsync(int imageId)
    {
        // Implementation would typically involve:
        // 1. Get image from database
        // 2. Delete physical file
        // 3. Remove database record
        // This is a placeholder - actual implementation depends on DbContext injection
        return Task.FromResult(true);
    }
    
    public Task<bool> SetPrimaryImageAsync(int itemId, int imageId)
    {
        // Implementation would set the specified image as primary
        // and unset any other primary images for this item
        return Task.FromResult(true);
    }
    
    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or not provided.");
        }
        
        if (file.Length > MaxFileSize)
        {
            throw new ArgumentException($"File size exceeds the maximum allowed size of {MaxFileSize / (1024 * 1024)}MB.");
        }
        
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
        }
    }
}
