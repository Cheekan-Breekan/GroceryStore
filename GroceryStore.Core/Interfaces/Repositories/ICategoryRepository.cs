using GroceryStore.Core.DTO;

namespace GroceryStore.Core.Interfaces.Repositories;
public interface ICategoryRepository
{
    Task<List<CategoryDto>?> GetCategoriesAsync(bool includeDeleted);
    Task<CategoryDto?> GetCategoryAsync(int id);
    Task<bool> AddCategoryAsync(CategoryDto category);
    Task<bool> UpdateCategoryAsync(CategoryDto category);
    Task<bool> DeleteOrRestoreCategoryAsync(int id, bool isToDelete);
    Task<int> CountCategoriesAsync(string searchFilter, bool includeDeleted);
    Task<bool> CategoryExistsByNameAsync(string name);
    Task<bool> CategoryExistsByIdAsync(int id);
    Task<List<CategoryDto>?> GetCategoriesByFilterSortPagingAsync(string searchFilter, string sortOrder, int page, int tablePageSize, bool includeDeleted);
}
