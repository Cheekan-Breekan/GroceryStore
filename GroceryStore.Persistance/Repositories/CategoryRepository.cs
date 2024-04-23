using GroceryStore.Core.DTO;
using GroceryStore.Core.Entities;
using GroceryStore.Core.Interfaces.Repositories;
using GroceryStore.Persistance.Repositories.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace GroceryStore.Persistance.Repositories;
public class CategoryRepository(AppBaseDbContext db, IDistributedCache cache) : ICategoryRepository
{
    private readonly AppBaseDbContext _db = db;
    private readonly IDistributedCache _cache = cache;
    private readonly static string _cacheAllCategoriesKey = "Categories-With-Deleted";
    private readonly static string _cacheCategoriesKey = "Categories";
    private readonly static string _cacheCategoriesCount = "Categories-Count";
    public async Task<List<CategoryDto>?> GetCategoriesAsync(bool includeDeleted)
    {
        var cacheName = includeDeleted ? _cacheAllCategoriesKey : _cacheCategoriesKey;
        var categories = await _cache.GetRecordAsync<List<CategoryDto>>(cacheName);
        if (categories is null)
        {
            var query = _db.Categories.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            categories = await query.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductsCount = c.Products.Count,
                IsDeleted = c.IsDeleted
            }).ToListAsync();

            if (categories is not null)
            {
                await _cache.SetRecordAsync(cacheName, categories, TimeSpan.FromMinutes(1));
            }
        }
        return categories;
    }
    public async Task<CategoryDto?> GetCategoryAsync(int id)
    {
        var category = await _cache.GetRecordAsync<CategoryDto>(CacheNames.Category(id));
        if (category is null)
        {
            category = await _db.Categories.Where(c => c.Id == id).Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductsCount = c.Products.Count,
                IsDeleted = c.IsDeleted
            }).FirstOrDefaultAsync();
            if (category is not null)
            {
                await _cache.SetRecordAsync(CacheNames.Category(id), category, TimeSpan.FromMinutes(1));
            }
        }
        return category;
    }
    public async Task<bool> AddCategoryAsync(CategoryDto category)
    {
        var categoryEntity = CategoryDto.ToCategory(category);
        await _db.Categories.AddAsync(categoryEntity);
        await _db.SaveChangesAsync();
        var result = categoryEntity.Id != 0;
        if (result)
        {
            await _cache.RemoveAsync(_cacheCategoriesKey);
            await _cache.RemoveAsync(_cacheAllCategoriesKey);
            await _cache.RemoveAsync(_cacheCategoriesCount);
        }
        return result;
    }
    public async Task<bool> UpdateCategoryAsync(CategoryDto category)
    {
        var categoryEntity = await _db.Categories.FindAsync(category.Id);
        if (categoryEntity != null)
        {
            categoryEntity.Name = category.Name;
            categoryEntity.Description = category.Description;
        }
        var result = await _db.SaveChangesAsync() > 0;
        if (result)
        {
            await _cache.RemoveAsync(CacheNames.Category(categoryEntity.Id));
            await _cache.RemoveAsync(_cacheCategoriesKey);
            await _cache.RemoveAsync(_cacheAllCategoriesKey);
        }
        return result;
    }
    public async Task<bool> DeleteOrRestoreCategoryAsync(int id, bool isToDelete)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
        {
            return false;
        }
        if (isToDelete && await _db.Products.Where(p => p.Category.Id == id && !p.IsDeleted).AnyAsync())
        {
            return false;
        }
        category.IsDeleted = isToDelete;
        var result = await _db.SaveChangesAsync() > 0;
        if (result)
        {
            await _cache.RemoveAsync(CacheNames.Category(id));
            await _cache.RemoveAsync(_cacheCategoriesKey);
            await _cache.RemoveAsync(_cacheAllCategoriesKey);
        }
        return result;
    }
    public async Task<int> CountCategoriesAsync(string searchFilter, bool includeDeleted)
    {
        var count = await _cache.GetRecordAsync<int>(_cacheCategoriesCount);
        if (count == default)
        {
            var categoriesQuery = _db.Categories.AsQueryable();
            categoriesQuery = ApplyFilters(searchFilter, includeDeleted, categoriesQuery);
            count = await categoriesQuery.CountAsync();
            if (count != default)
            {
                await _cache.SetRecordAsync(_cacheCategoriesCount, count, TimeSpan.FromMinutes(1));
            }
        }
        return count;
    }
    public async Task<bool> CategoryExistsByNameAsync(string name)
    {
        return await _db.Categories.AnyAsync(x => x.Name == name);
    }
    public async Task<bool> CategoryExistsByIdAsync(int id)
    {
        var category = await _cache.GetRecordAsync<CategoryDto>(CacheNames.Category(id));
        if (category is not null && !category.IsDeleted)
        {
            return true;
        }
        return await _db.Categories.AnyAsync(c => c.Id == id && !c.IsDeleted);
    }
    public async Task<List<CategoryDto>?> GetCategoriesByFilterSortPagingAsync(string searchFilter, string sortOrder, int page, int tablePageSize, bool includeDeleted)
    {
        var cacheName = $"Categories:search:{searchFilter}:sort:{sortOrder}:page:{page}:tps:{tablePageSize}";
        var categories = await _cache.GetRecordAsync<List<CategoryDto>>(cacheName);
        if (categories is not null)
        {
            return categories;
        }
        var categoriesQuery = _db.Categories.AsQueryable();
        categoriesQuery = ApplyFilters(searchFilter, includeDeleted, categoriesQuery);
        categoriesQuery = sortOrder switch
        {
            "name_desc" => categoriesQuery.OrderByDescending(c => c.Name),
            "deleted" => categoriesQuery.OrderBy(c => c.IsDeleted),
            "deleted_desc" => categoriesQuery.OrderByDescending(c => c.IsDeleted),
            "products" => categoriesQuery.OrderBy(c => c.Products.Count),
            "products_desc" => categoriesQuery.OrderByDescending(c => c.Products.Count),
            _ => categoriesQuery.OrderBy(c => c.Name),
        };
        categories = await categoriesQuery.Skip((page - 1) * tablePageSize).Take(tablePageSize).Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            ProductsCount = c.Products.Count,
            IsDeleted = c.IsDeleted
        }).ToListAsync();
        if (categories is not null)
        {
            await _cache.SetRecordAsync(cacheName, categories, TimeSpan.FromMinutes(1));
        }
        return categories;
    }

    private static IQueryable<Category> ApplyFilters(string searchFilter, bool includeDeleted, IQueryable<Category> categoriesQuery)
    {
        if (!includeDeleted)
        {
            categoriesQuery = categoriesQuery.Where(x => !x.IsDeleted);
        }
        if (!string.IsNullOrWhiteSpace(searchFilter))
        {
            categoriesQuery = categoriesQuery.Where(x => x.Name.Contains(searchFilter));
        }
        return categoriesQuery;
    }
}
