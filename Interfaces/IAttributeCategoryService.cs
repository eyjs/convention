namespace LocalRAG.Interfaces;

public interface IAttributeCategoryService
{
    Task<List<object>> GetCategoriesAsync(int conventionId);
    Task<object> CreateCategoryAsync(int conventionId, string name, string? icon, List<string> attributeKeys, int? orderNum = null);
    Task<object> UpdateCategoryAsync(int categoryId, string name, string? icon, List<string> attributeKeys, int? orderNum = null);
    Task<bool> DeleteCategoryAsync(int categoryId);
    Task<List<string>> GetAttributeKeysAsync(int conventionId);
    Task<object> GetGroupedAttributesAsync(int conventionId, int userId);
    Task ReorderCategoriesAsync(List<int> categoryIds);
}
