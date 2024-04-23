using GroceryStore.Core.DTO;
using GroceryStore.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class ProductsEditorController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    public ProductsEditorController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }
    [Route("GetProducts")]
    [HttpGet]
    public async Task<IActionResult> GetProducts(string? sortOrder = null, string? searchFilter = null, string? currentFilter = null,
        int categoryFilter = 0, int page = 1, int tablePageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(searchFilter))
        {
            searchFilter = currentFilter;
        }
        else
        {
            page = 1;
        }
        page = Math.Max(1, page);
        var products = await _productService.GetProductsByFilterSortPagingAsync(
            searchFilter, categoryFilter, sortOrder, page, tablePageSize, includeDeleted: true);
        return Ok(products);
    }
    [Route("CountProducts")]
    [HttpGet]
    public async Task<IActionResult> CountProducts(string? searchFilter = null, int categoryFilter = 0)
    {
        var result = await _productService.CountProductsAsync(searchFilter, categoryFilter, includeDeleted: true);
        return Ok(result);
    }
    [Route("AddProduct")]
    [HttpPost]
    public async Task<IActionResult> AddProduct(ProductInfoDto productInfo)
    {
        var category = await _categoryService.GetCategoryAsync(productInfo.CategoryId);
        if (category is null)
        {
            return BadRequest($"Selected category with id:{productInfo.CategoryId} doesn't exist.");
        }

        var product = ProductInfoDto.ToProductDto(productInfo);
        product.Category = category;

        var result = await _productService.AddProductAsync(product);
        return result ? Ok() : BadRequest($"Failed to add new product {productInfo.Name}.");
    }
    [Route("GetProduct")]
    [HttpGet]
    public async Task<IActionResult> GetProduct(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product is null)
        {
            return NotFound();
        }
        return Ok(product);
    }
    [Route("EditProduct")]
    [HttpPost]
    public async Task<IActionResult> EditProduct(ProductInfoDto product)
    {
        var result = await _productService.UpdateProductInfoAsync(product);
        return result ? Ok() : BadRequest($"Failed to update product {product.Name}");
    }
    [Route("AddMainImage")]
    [HttpPost]
    public async Task<IActionResult> AddMainImage(IFormFile file, int productId)
    {
        if (file is null)
        {
            return BadRequest("Sent image is null.");
        }
        var productDto = new ProductImagesDto
        {
            Id = productId,
            ImageMainFile = file,
            IsMain = true
        };
        var result = await _productService.AddProductImagesAsync(productDto);
        return result ? Ok() : BadRequest("Failed to add new main image for product.");
    }
    [Route("AddImages")]
    [HttpPost]
    public async Task<IActionResult> AddImages(IFormFileCollection files, int productId)
    {
        if (!files.Any())
        {
            return BadRequest("Sent image collection is empty.");
        }
        var imagesDto = new ProductImagesDto
        {
            Id = productId,
            ImageFiles = files,
        };
        var result = await _productService.AddProductImagesAsync(imagesDto);
        return result ? Ok() : BadRequest("Failed to add product images.");
    }
    [Route("DeleteImage")]
    [HttpPost]
    public async Task<IActionResult> DeleteImage(string imageName, int productId)
    {
        if (string.IsNullOrEmpty(imageName) || productId < 1)
        {
            return BadRequest();
        }
        var result = await _productService.DeleteProductImageAsync(imageName, productId);
        return result ? Ok() : BadRequest($"Failed to delete image {imageName} of product with id:{productId}");
    }
    [Route("DeleteProduct")]
    [HttpPost]
    public async Task<IActionResult> DeleteProduct(int productId)
    {
        var result = await _productService.DeleteOrRestoreProductAsync(productId, isToDelete: true);
        return result ? Ok() : BadRequest($"Failed to delete product with id:{productId}");
    }
    [Route("RestoreProduct")]
    [HttpPost]
    public async Task<IActionResult> RestoreProduct(int productId)
    {
        var result = await _productService.DeleteOrRestoreProductAsync(productId, isToDelete: false);
        return result ? Ok() : BadRequest($"Failed to restore product with id:{productId}");
    }
}
