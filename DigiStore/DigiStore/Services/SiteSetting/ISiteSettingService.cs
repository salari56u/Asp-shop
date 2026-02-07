public interface ISiteSettingService
{
    Task<string> GetValue(string key, string defaultValue = "");
    Task<Dictionary<string, string>> GetAllSettings();
    Task<List<Category>> GetCategoriesForMenu();

}
