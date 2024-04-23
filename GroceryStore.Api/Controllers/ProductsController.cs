using GroceryStore.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("GetProducts")]
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
            searchFilter, categoryFilter, sortOrder, page, tablePageSize, includeDeleted: false);
        return Ok(products);
    }

    [HttpGet("CountProducts")]
    public async Task<IActionResult> CountProducts(string? searchFilter = null, int categoryFilter = 0)
    {
        var result = await _productService.CountProductsAsync(searchFilter, categoryFilter, includeDeleted: false);
        return Ok(result);
    }

    [HttpGet("GetProduct")]
    public async Task<IActionResult> GetProduct(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product is null)
        {
            return NotFound("Данный продукт не найден");
        }
        return Ok(product);
    }
}
