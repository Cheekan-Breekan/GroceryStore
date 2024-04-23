using GroceryStore.Core.DTO;

namespace GroceryStore.Core.Interfaces.Services;
public interface IProductService
{
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<bool> AddProductAsync(ProductDto product);
    Task<bool> UpdateProductInfoAsync(ProductInfoDto product);
    Task<bool> AddProductImagesAsync(ProductImagesDto product);
    Task<bool> DeleteProductImageAsync(string imageName, int productId);
    Task<bool> DeleteOrRestoreProductAsync(int id, bool isToDelete);
    Task<int> CountProductsAsync(string? searchFilter = null, int categoryFilter = 0, bool includeDeleted = true);
    Task<List<ProductDto>?> GetProductsByFilterSortPagingAsync(string searchFilter, int categoryFilter, string sortOrder,
        int page, int tablePageSize, bool includeDeleted);
}
