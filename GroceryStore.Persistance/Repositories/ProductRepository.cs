using GroceryStore.Core.DTO;
using GroceryStore.Core.Entities;
using GroceryStore.Core.Interfaces.Repositories;
using GroceryStore.Persistance.Repositories.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace GroceryStore.Persistance.Repositories;
public class ProductRepository(AppBaseDbContext db, IDistributedCache cache) : IProductRepository
{
    private readonly AppBaseDbContext _db = db;
    private readonly IDistributedCache _cache = cache;

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _cache.GetRecordAsync<ProductDto>(CacheNames.Product(id));
        if (product is null)
        {
            product = await _db.Products.Where(p => p.Id == id).Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Quantity = p.Quantity,
                Category = new CategoryDto
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                    Description = p.Category.Description,
                    ProductsCount = p.Category.Products.Count
                },
                CategoryId = p.Category.Id,
                OrdersCount = p.OrdersCount,
                ImageMainPath = p.ImageMainPath,
                ImagePaths = p.ImagePaths,
                IsDeleted = p.IsDeleted
            }).FirstOrDefaultAsync();
            if (product is not null)
            {
                await _cache.SetRecordAsync(CacheNames.Product(id), product, TimeSpan.FromMinutes(1));
            }
        }
        return product;
    }
    public async Task<bool> AddProductAsync(ProductDto product)
    {
        var entity = ProductDto.ToProduct(product);
        _db.Products.Attach(entity);
        return await _db.SaveChangesAsync() > 0;
    }
    public async Task<bool> UpdateProductInfoAsync(ProductInfoDto product)
    {
        var entity = await _db.Products.Include(p => p.Category).FirstAsync(p => product.Id == p.Id);
        entity.Name = product.Name;
        entity.Description = product.Description;
        entity.Price = product.Price;
        entity.Quantity = product.Quantity;
        if (entity.Category.Id != product.CategoryId)
        {
            var category = await _db.Categories.FindAsync(product.CategoryId);
            entity.Category = category;
        }
        return await SaveAndRemoveCache(entity.Id);
    }
    public async Task<bool> AddProductImagesAsync(ProductImagesDto product)
    {
        var entity = await _db.Products.FindAsync(product.Id);
        if (product.IsMain)
        {
            entity.ImageMainPath = product.ImageMainPath;
        }
        else
        {
            entity.ImagePaths.AddRange(product.ImagePaths);
        }
        return await SaveAndRemoveCache(entity.Id);
    }
    public async Task<bool> DeleteImageAsync(int productId, string imageName)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product.ImageMainPath == imageName)
        {
            product.ImageMainPath = null;
        }
        else if (product.ImagePaths.Contains(imageName))
        {
            product.ImagePaths.Remove(imageName);
        }
        else
        {
            return false;
        }
        return await SaveAndRemoveCache(product.Id);
    }
    public async Task<bool> DeleteOrRestoreProductAsync(int id, bool isToDelete)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
        {
            return false;
        }
        product.IsDeleted = isToDelete;
        return await SaveAndRemoveCache(product.Id);
    }
    public async Task<int> CountProductsAsync(string searchFilter, int categoryFilter, bool includeDeleted)
    {
        var count = await _cache.GetRecordAsync<int>($"Products-count:search{searchFilter}:category:{categoryFilter}:del:{includeDeleted}");
        if (count == default)
        {
            var productsQuery = _db.Products.AsQueryable();
            productsQuery = ApplyGetProductsFilters(productsQuery, searchFilter, categoryFilter, includeDeleted);
            count = await productsQuery.CountAsync();
            if (count != default)
            {
                await _cache.SetRecordAsync(
                    $"Products-count:search{searchFilter}:category:{categoryFilter}:del:{includeDeleted}", count, TimeSpan.FromMinutes(1));
            }
        }
        return count;
    }
    public async Task<List<ProductDto>?> GetProductsByFilterSortPagingAsync(string searchFilter, int categoryFilter, string sortOrder,
        int page, int tablePageSize, bool includeDeleted)
    {
        var products = await _cache.GetRecordAsync<List<ProductDto>>(
            $"Products.search:{searchFilter}:category:{categoryFilter}:sort:{sortOrder}:page:{page}:tps:{tablePageSize}:del:{includeDeleted}");
        if (products is not null)
        {
            return products;
        }

        var productsQuery = _db.Products.AsQueryable();
        productsQuery = ApplyGetProductsFilters(productsQuery, searchFilter, categoryFilter, includeDeleted);
        productsQuery = sortOrder switch
        {
            "name_desc" => productsQuery.OrderByDescending(p => p.Name),
            "category" => productsQuery.OrderBy(p => p.Category.Name),
            "category_desc" => productsQuery.OrderByDescending(p => p.Category.Name),
            "price" => productsQuery.OrderBy(p => p.Price),
            "price_desc" => productsQuery.OrderByDescending(p => p.Price),
            "quantity" => productsQuery.OrderBy(p => p.Quantity),
            "quantity_desc" => productsQuery.OrderByDescending(p => p.Quantity),
            "ordersCount" => productsQuery.OrderBy(p => p.OrdersCount),
            "ordersCount_desc" => productsQuery.OrderByDescending(p => p.OrdersCount),
            _ => productsQuery.OrderBy(p => p.Name),
        };

        products = await productsQuery.Skip((page - 1) * tablePageSize)
            .Take(tablePageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Quantity = p.Quantity,
                Category = new CategoryDto
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                    Description = p.Category.Description,
                    ProductsCount = p.Category.Products.Count
                },
                CategoryId = p.Category.Id,
                OrdersCount = p.OrdersCount,
                ImageMainPath = p.ImageMainPath,
                ImagePaths = p.ImagePaths
            })
            .ToListAsync();
        if (products is not null)
        {
            await _cache.SetRecordAsync(
                $"Products.search:{searchFilter}:category:{categoryFilter}:sort:{sortOrder}:page:{page}:tps:{tablePageSize}:del:{includeDeleted}",
                products,
                TimeSpan.FromMinutes(1));
        }
        return products;
    }
    private IQueryable<Product> ApplyGetProductsFilters(IQueryable<Product> productsQuery, string searchFilter, int categoryFilter, bool includeDeleted)
    {
        if (!includeDeleted)
        {
            productsQuery = productsQuery.Where(p => !p.IsDeleted);
        }
        if (categoryFilter != 0)
        {
            productsQuery = productsQuery.Where(p => p.Category.Id == categoryFilter);
        }
        if (!string.IsNullOrWhiteSpace(searchFilter))
        {
            productsQuery = productsQuery.Where(x => x.Name.Contains(searchFilter));
        }
        return productsQuery;
    }
    private async Task<bool> SaveAndRemoveCache(int id)
    {
        var result = await _db.SaveChangesAsync() > 0;
        if (result)
        {
            await _cache.RemoveAsync(CacheNames.Product(id));
        }
        return result;
    }
}
