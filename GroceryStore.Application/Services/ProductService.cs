using GroceryStore.Core.DTO;
using GroceryStore.Core.Interfaces;
using GroceryStore.Core.Interfaces.Repositories;
using GroceryStore.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GroceryStore.Application.Services;
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepo;
    private readonly ILogger<ProductService> _logger;
    private readonly IFileService _fileService;

    public ProductService(IProductRepository repository, ILogger<ProductService> logger, IFileService fileService)
    {
        _productRepo = repository;
        _logger = logger;
        _fileService = fileService;
    }
    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        try
        {
            return await _productRepo.GetProductByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return null;
        }
    }
    public async Task<bool> AddProductAsync(ProductDto product)
    {
        try
        {
            if (product.ImageMain is not null)
            {
                var path = _fileService.UploadImage(product.ImageMain);
                product.ImageMainPath = path;
            }
            if (product.Images?.Count > 0)
            {
                foreach (var image in product.Images)
                {
                    var path = _fileService.UploadImage(image);
                    product.ImagePaths.Add(path);
                }
            }
            return await _productRepo.AddProductAsync(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return false;
        }
    }
    public async Task<bool> UpdateProductInfoAsync(ProductInfoDto product)
    {
        try
        {
            return await _productRepo.UpdateProductInfoAsync(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return false;
        }
    }
    public async Task<bool> AddProductImagesAsync(ProductImagesDto product)
    {
        try
        {
            if (product.IsMain)
            {
                var productEntity = await _productRepo.GetProductByIdAsync(product.Id);
                if (!string.IsNullOrEmpty(productEntity.ImageMainPath))
                {
                    _fileService.DeleteImage(productEntity.ImageMainPath);
                }
                product.ImageMainPath = _fileService.UploadImage(product.ImageMainFile);
            }
            else
            {
                foreach (var file in product.ImageFiles)
                {
                    var newImagePath = _fileService.UploadImage(file);
                    product.ImagePaths.Add(newImagePath);
                }
            }
            await _productRepo.AddProductImagesAsync(product);
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return false;
        }
    }
    public async Task<bool> DeleteProductImageAsync(string imageName, int productId)
    {
        try
        {
            var result = await _productRepo.DeleteImageAsync(productId, imageName);
            if (!result)
            {
                return false;
            }
            _fileService.DeleteImage(imageName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return false;
        }
    }
    public async Task<bool> DeleteOrRestoreProductAsync(int id, bool isToDelete)
    {
        try
        {
            return await _productRepo.DeleteOrRestoreProductAsync(id, isToDelete);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return false;
        }
    }
    public async Task<int> CountProductsAsync(string? searchFilter = null, int categoryFilter = 0, bool includeDeleted = true)
    {
        try
        {
            return await _productRepo.CountProductsAsync(searchFilter, categoryFilter, includeDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return default;
        }
    }

    public async Task<List<ProductDto>?> GetProductsByFilterSortPagingAsync(string searchFilter, int categoryFilter, string sortOrder,
        int page, int tablePageSize, bool includeDeleted)
    {
        try
        {
            return await _productRepo.GetProductsByFilterSortPagingAsync(searchFilter, categoryFilter, sortOrder, page, tablePageSize, includeDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return new List<ProductDto>(0);
        }
    }
}
