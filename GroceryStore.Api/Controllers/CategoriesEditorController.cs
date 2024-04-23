using GroceryStore.Core.DTO;
using GroceryStore.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class CategoriesEditorController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesEditorController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    [Route("GetCategories")]
    [HttpGet]
    public async Task<IActionResult> GetCategories(string? sortOrder = null, string? searchFilter = null, string? currentFilter = null, int page = 1, int tablePageSize = 20)
    {
        if (!string.IsNullOrWhiteSpace(searchFilter))
        {
            page = 1;
        }
        else
        {
            searchFilter = currentFilter;
        }
        page = page < 1 ? 1 : page;
        var categories = await _categoryService.GetCategoriesByFilterSortPagingAsync(searchFilter, sortOrder, page, tablePageSize, includeDeleted: true);
        return Ok(categories);
    }
    [Route("CountCategories")]
    [HttpGet]
    public async Task<IActionResult> CountCategories(string? searchFilter = null)
    {
        var result = await _categoryService.CountCategoriesAsync(searchFilter, includeDeleted: true);
        return Ok(result);
    }
    [Route("AddCategory")]
    [HttpPost]
    public async Task<IActionResult> AddCategory(CategoryDto category)
    {
        if (await _categoryService.CategoryExistsByNameAsync(category.Name))
        {
            return BadRequest($"Category {category.Name} already exists.");
        }
        var result = await _categoryService.AddCategoryAsync(category);
        return result ? Ok() : BadRequest($"Failed to add category {category.Name}.");
    }
    [Route("GetCategory")]
    [HttpGet]
    public async Task<IActionResult> GetCategory(int categoryId)
    {
        var category = await _categoryService.GetCategoryAsync(categoryId);
        return category is not null ? Ok(category) : NotFound();
    }
    [Route("EditCategory")]
    [HttpPost]
    public async Task<IActionResult> EditCategory(CategoryDto category)
    {
        if (category.Id <= 0)
        {
            return BadRequest($"{category.Id} is invalid category id.");
        }
        var result = await _categoryService.UpdateCategoryAsync(category);
        return result ? Ok(category) : BadRequest($"Failed to update category {category.Name}.");
    }
    [Route("DeleteOrRestoreCategory")]
    [HttpPost]
    public async Task<IActionResult> DeleteOrRestoreCategory(int categoryId, bool isToDelete)
    {
        if (categoryId <= 0)
        {
            return BadRequest($"Invalid category id:{categoryId}.");
        }
        var result = await _categoryService.DeleteOrRestoreCategoryAsync(categoryId, isToDelete);
        return result ? Ok() : BadRequest("Failed to modify delete/restore property of category.");
    }
}
