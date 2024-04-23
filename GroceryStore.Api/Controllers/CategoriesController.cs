using GroceryStore.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    public CategoriesController(ICategoryService categoryService)
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
        var categories = await _categoryService.GetCategoriesByFilterSortPagingAsync(searchFilter, sortOrder, page, tablePageSize, includeDeleted: false);
        return Ok(categories);
    }
    [Route("CountCategories")]
    [HttpGet]
    public async Task<IActionResult> CountCategories(string? searchFilter = null)
    {
        var result = await _categoryService.CountCategoriesAsync(searchFilter, includeDeleted: false);
        return Ok(result);
    }
    [Route("GetCategory")]
    [HttpGet]
    public async Task<IActionResult> GetCategory(int categoryId)
    {
        var category = await _categoryService.GetCategoryAsync(categoryId);
        return category is not null ? Ok(category) : NotFound();
    }
}
