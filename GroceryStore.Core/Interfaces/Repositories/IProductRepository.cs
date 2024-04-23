using GroceryStore.Core.DTO;

namespace GroceryStore.Core.Interfaces.Repositories;
public interface IProductRepository
{
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<bool> AddProductAsync(ProductDto product);
    Task<bool> UpdateProductInfoAsync(ProductInfoDto product);
    Task<bool> AddProductImagesAsync(ProductImagesDto product);
    Task<bool> DeleteImageAsync(int productId, string imagePath);
    Task<int> CountProductsAsync(string searchFilter, int categoryFilter, bool includeDeleted);
    Task<bool> DeleteOrRestoreProductAsync(int id, bool isToDelete);
    Task<List<ProductDto>?> GetProductsByFilterSortPagingAsync(string searchFilter, int categoryFilter, string sortOrder,
        int page, int tablePageSize, bool includeDeleted);
}
