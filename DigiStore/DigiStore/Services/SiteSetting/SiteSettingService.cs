using DigiStore.Data;
using Microsoft.EntityFrameworkCore;

public class SiteSettingService : ISiteSettingService
{
    private readonly AppDbContext _context;
    private static Dictionary<string, string> _cachedSettings;

    public SiteSettingService(AppDbContext context)
    {
        _context = context;
    }



    public async Task<Dictionary<string, string>> GetAllSettings()
    {
        if (_cachedSettings != null) return _cachedSettings;

        _cachedSettings = await _context.SiteSettings
            .AsNoTracking()
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        return _cachedSettings;
    }
    public async Task<string> GetValue(string key, string defaultValue = "")
    {
        var setting = await _context.SiteSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == key);

        return setting?.Value ?? defaultValue;
    }
    public async Task<List<Category>> GetCategoriesForMenu()
    {
        return await _context.Categories
            .Include(c => c.Children) 
            .Where(c => c.ParentCategoryId == null) 
            .AsNoTracking()
            .ToListAsync();
    }
    public static void ClearCache()
    {
        _cachedSettings = null;
    }
}