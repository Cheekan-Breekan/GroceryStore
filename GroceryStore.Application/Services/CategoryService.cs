using GroceryStore.Core.DTO;
using GroceryStore.Core.Interfaces.Repositories;
using GroceryStore.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GroceryStore.Application.Services;
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly ILogger<CategoryService> _logger;
    public CategoryService(ICategoryRepository categoryRepo, ILogger<CategoryService> logger)
    {
        _categoryRepo = categoryRepo;
        _logger = logger;
    }
    public async Task<List<CategoryDto>?> GetCategoriesAsync(bool includeDeleted = false)
    {
        try
        {
            return await _categoryRepo.GetCategoriesAsync(includeDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return null;
        }
    }
    public async Task<CategoryDto?> GetCategoryAsync(int id)
    {
        try
        {
            return await _categoryRepo.GetCategoryAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return null;
        }
    }
    public async Task<bool> AddCategoryAsync(CategoryDto category)
    {
        try
        {
            return await _categoryRepo.AddCategoryAsync(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return false;
        }
    }
    public async Task<bool> UpdateCategoryAsync(CategoryDto category)
    {
        try
        {
            return await _categoryRepo.UpdateCategoryAsync(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return false;
        }
    }
    public async Task<bool> DeleteOrRestoreCategoryAsync(int id, bool isToDelete)
    {
        try
        {
            return await _categoryRepo.DeleteOrRestoreCategoryAsync(id, isToDelete);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return false;
        }
    }
    public async Task<int> CountCategoriesAsync(string searchFilter, bool includeDeleted)
    {
        try
        {
            return await _categoryRepo.CountCategoriesAsync(searchFilter, includeDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return default;
        }
    }
    public async Task<bool> CategoryExistsByNameAsync(string name)
    {
        try
        {
            return await _categoryRepo.CategoryExistsByNameAsync(name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return true;
        }
    }
    public async Task<bool> CategoryExistsByIdAsync(int id)
    {
        try
        {
            return await _categoryRepo.CategoryExistsByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return true;
        }
    }
    public async Task<List<CategoryDto>?> GetCategoriesByFilterSortPagingAsync(string searchFilter, string sortOrder, int page, int tablePageSize, bool includeDeleted)
    {
        try
        {
            return await _categoryRepo.GetCategoriesByFilterSortPagingAsync(searchFilter, sortOrder, page, tablePageSize, includeDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return new List<CategoryDto>(0);
        }
    }
}
