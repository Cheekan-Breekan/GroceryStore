using GroceryStore.Core.Interfaces.Services;
using GroceryStore.MVC.Extensions;
using GroceryStore.MVC.Filters;
using GroceryStore.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GroceryStore.MVC.Controllers;
public class HomeController : Controller
{
    private readonly int productsPerLoad = 20;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ICartService _cartService;

    public HomeController(IProductService productService, ICategoryService categoryService,
        ICartService cartService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _cartService = cartService;
    }
    [DisplayNotification]
    public async Task<IActionResult> Index(string sortOrder, string searchFilter, string currentFilter, int categoryFilter = 0)
    {
        var categories = await _categoryService.GetCategoriesAsync();
        if (categories?.Count < 1)
        {
            ViewBag.Notification = new NotificationVM("Произошла внутрення ошибка! Попробуйте перезагрузить страницу.", true);
        }
        if (categoryFilter > 0 && categories.Any(c => c.Id == categoryFilter))
        {
            ViewBag.CurrentSelectedCategory = categoryFilter;
        }
        ViewBag.CurrentSortOrder = sortOrder;
        ViewBag.NameSortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "name_desc" : string.Empty;
        ViewBag.CategorySortOrder = sortOrder == "category" ? "category_desc" : "category";
        ViewBag.PriceSortOrder = sortOrder == "price" ? "price_desc" : "price";
        ViewBag.QuantitySortOrder = sortOrder == "quantity" ? "quantity_desc" : "quantity";
        ViewBag.OrdersCountSortOrder = sortOrder == "ordersCount" ? "ordersCount_desc" : "ordersCount";
        if (string.IsNullOrWhiteSpace(searchFilter))
        {
            searchFilter = currentFilter;
        }
        ViewBag.CurrentSearchFilter = searchFilter;

        return View(categories);
    }

    [HttpPost]
    public async Task<IActionResult> ProductsLoad(string sortOrder, string searchFilter, string currentFilter, int categoryFilter = 0, int firstItem = 0)
    {
        if (categoryFilter > 0 && await _categoryService.CategoryExistsByIdAsync(categoryFilter))
        {
            ViewBag.CurrentSelectedCategory = categoryFilter;
        }
        if (string.IsNullOrWhiteSpace(searchFilter))
        {
            searchFilter = currentFilter;
        }

        var countProducts = await _productService.CountProductsAsync(searchFilter, categoryFilter, includeDeleted: false);
        if (countProducts <= firstItem)
        {
            return BadRequest("maxCount");
        }

        var page = (firstItem / productsPerLoad) + 1;
        var products = await _productService.GetProductsByFilterSortPagingAsync(searchFilter, categoryFilter, sortOrder,
            page, productsPerLoad, includeDeleted: false);
        var productsVm = new List<ProductVM>();

        if (User.Identity.IsAuthenticated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var productsIdsInCart = _cartService.GetProductsIdsInCart(userId);
            foreach (var product in products)
            {
                var productVm = ProductVM.FromDto(product);
                if (productsIdsInCart.TryGetValue(product.Id, out var productId))
                {
                    productVm.IsInCart = true;
                }
                productsVm.Add(productVm);
            }
        }
        else
        {
            productsVm = products.Select(p => ProductVM.FromDto(p)).ToList();
        }

        return PartialView(productsVm);
    }
    public async Task<IActionResult> ProductInfo(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product is null)
        {
            AddTempDataForNotification("Произошла ошибка! Данный продукт не был найден.", false);
            return RedirectToAction(nameof(Index));
        }
        if (User.Identity.IsAuthenticated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItemIdAndQuantity = await _cartService.GetCartItemIdAndQuantityAsync(productId, userId);
            ViewBag.CartItemIdAndQuantity = cartItemIdAndQuantity;
        }
        return View(product);
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddProductToCart(int productId, int productQuantity)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _cartService.AddToCartAsync(userId, productId, productQuantity);
        if (result)
        {
            return PartialView("_ProductAlreadyInCartPartial");
        }
        return BadRequest(new NotificationVM("Произошла ошибка! Продукт не был добавлен в корзину.", true));
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddProductToCartFromInfo(int productId, int productQuantity)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _cartService.AddToCartAsync(userId, productId, productQuantity);
        if (result)
        {
            var cartItemIdAndQuantity = await _cartService.GetCartItemIdAndQuantityAsync(productId, userId);
            if (cartItemIdAndQuantity is null) { RedirectToAction(nameof(ProductInfo)); }
            var vm = new CartItemQuantityVM
            {
                Quantity = cartItemIdAndQuantity.Quantity,
                CartItemId = cartItemIdAndQuantity.Id
            };
            return PartialView("_CartItemQuantityPartial", vm);
        }
        return BadRequest(new NotificationVM("Произошла ошибка! Продукт не был добавлен в корзину.", true));
    }

    public IActionResult About()
    {
        return View();
    }

    private void AddTempDataForNotification(string message, bool IsSuccessMessage)
    {
        TempData[ControllerExt.NotiNameForTempData] = message;
        if (!IsSuccessMessage) { TempData[ControllerExt.ErrorNameForTempData] = true; }
    }
}
