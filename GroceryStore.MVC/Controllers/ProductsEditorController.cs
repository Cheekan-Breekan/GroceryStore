using GroceryStore.Core.DTO;
using GroceryStore.Core.Interfaces.Services;
using GroceryStore.MVC.Extensions;
using GroceryStore.MVC.Filters;
using GroceryStore.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.MVC.Controllers;
[Authorize(Roles = "Admin")]
public class ProductsEditorController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly int tablePageSize = 20;

    public ProductsEditorController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    [DisplayNotification]
    public async Task<IActionResult> Index(string sortOrder, string searchFilter, string currentFilter, int categoryFilter = 0, int page = 1)
    {
        var categories = await GetCategories();
        ViewBag.Categories = categories;
        if (categoryFilter > 0 && categories.Any(c => c.Id == categoryFilter))
        {
            ViewBag.CurrentSelectedCategory = categoryFilter;
        }
        else
        {
            categoryFilter = 0;
        }

        ViewBag.CurrentSortOrder = sortOrder;
        ViewBag.NameSortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "name_desc" : string.Empty;
        ViewBag.CategorySortOrder = sortOrder == "category" ? "category_desc" : "category";
        ViewBag.PriceSortOrder = sortOrder == "price" ? "price_desc" : "price";
        ViewBag.QuantitySortOrder = sortOrder == "quantity" ? "quantity_desc" : "quantity";
        ViewBag.OrdersCountSortOrder = sortOrder == "ordersCount" ? "ordersCount_desc" : "ordersCount";
        if (!string.IsNullOrWhiteSpace(searchFilter))
        {
            page = 1;
        }
        else
        {
            searchFilter = currentFilter;
        }
        ViewBag.CurrentSearchFilter = searchFilter;

        page = page < 1 ? 1 : page;
        var productsCount = await _productService.CountProductsAsync(searchFilter, categoryFilter);
        var pager = new Pager(productsCount, page, ControllerExt.NameOf<ProductsEditorController>(), nameof(Index), tablePageSize);
        ViewBag.Pager = pager;

        var products = await _productService.GetProductsByFilterSortPagingAsync(searchFilter, categoryFilter, sortOrder,
            page, tablePageSize, includeDeleted: true);

        return View(products);
    }
    public async Task<IActionResult> AddProduct()
    {
        ViewBag.Categories = await GetCategories();
        var product = new ProductDto();
        return View(product);
    }
    [HttpPost]
    public async Task<IActionResult> AddProduct(ProductDto product)
    {
        ViewBag.Categories = await GetCategories();
        if (!ModelState.IsValid)
        {
            ViewBag.Notification ??= new NotificationVM("Не удалось!", true);
            return View(product);
        }
        var category = await _categoryService.GetCategoryAsync(product.CategoryId);
        if (category is null)
        {
            ModelState.AddModelError(nameof(product), "Не удалось добавить продукт к выбранной категории!");
            ViewBag.Notification = new NotificationVM("Не удалось добавить продукт к выбранной категории!", true);
            return View(product);
        }
        product.Category = category;
        var result = await _productService.AddProductAsync(product);
        if (!result)
        {
            ViewBag.Notification = new NotificationVM("Произошла неустановленная внутренняя ошибка, пожалуйста, попробуйте ещё раз!", true);
            return View(product);
        }

        AddTempDataForNotification($"Продукт {product.Name} успешно создан!", true);
        return RedirectToAction(nameof(Index));
    }
    [DisplayNotification]
    public async Task<IActionResult> EditProduct(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product is null)
        {
            AddTempDataForNotification("Произошла неустановленная ошибка, пожалуйста, попробуйте ещё раз!", false);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Categories = await GetCategories();
        return View(product);
    }
    [HttpPost]
    public async Task<IActionResult> EditProduct(ProductDto product)
    {
        ViewBag.Categories = await GetCategories();
        if (!ModelState.IsValid)
        {
            ViewBag.Notification = new NotificationVM("Неправильно заполнена форма!", true);
            return View(product);
        }
        var editedProduct = ProductInfoDto.FromProductDto(product);
        var result = await _productService.UpdateProductInfoAsync(editedProduct);
        CheckResultAndCreateNotificationMessage($"Продукт {product.Name} успешно отредактирован!",
            "Произошла неустановленная внутренняя ошибка, пожалуйста, попробуйте ещё раз!", result);
        return RedirectToAction(nameof(EditProduct), new { productId = product.Id });
    }
    public async Task<IActionResult> AddMainImage(IFormFile image, int productId)
    {
        if (image is null)
        {
            AddTempDataForNotification("Вы не выбрали ни один файл!", false);
            return RedirectToAction(nameof(EditProduct), new { productId });
        }
        var productDto = new ProductImagesDto
        {
            Id = productId,
            ImageMainFile = image,
            IsMain = true
        };
        var result = await _productService.AddProductImagesAsync(productDto);
        CheckResultAndCreateNotificationMessage("Изображение успешно добавлено!", "Не удалось добавить изображение, пожалуйста, попробуйте ещё раз!", result);
        return RedirectToAction(nameof(EditProduct), new { productId });
    }
    public async Task<IActionResult> AddImages(IFormFileCollection formCollection, int productId)
    {
        if (formCollection?.Count == 0)
        {
            AddTempDataForNotification("Вы не выбрали ни один файл!", false);
            ModelState.TryAddModelError("ImageFiles", "Вы не выбрали ни один файл!");
            return RedirectToAction(nameof(EditProduct), new { productId });
        }
        if (formCollection?.Count > 10)
        {
            AddTempDataForNotification("Максимально возможное количество файлов: 10!", false);
            ModelState.TryAddModelError("ImageFiles", "Максимально возможное количество файлов: 10!");
            return RedirectToAction(nameof(EditProduct), new { productId });
        }
        var productDto = new ProductImagesDto
        {
            Id = productId,
            ImageFiles = formCollection
        };

        var result = await _productService.AddProductImagesAsync(productDto);
        CheckResultAndCreateNotificationMessage("Изображения успешно добавлены!", "Не удалось добавить изображения, пожалуйста, попробуйте ещё раз!", result);
        return RedirectToAction(nameof(EditProduct), new { productId });
    }
    public async Task<IActionResult> DeleteImage(string imagePath, int productId)
    {
        bool result = false;
        if (!string.IsNullOrWhiteSpace(imagePath) || productId != 0)
        {
            result = await _productService.DeleteProductImageAsync(imagePath, productId);
        }
        CheckResultAndCreateNotificationMessage("Изображение успешно удалено!", "Не удалось удалить изображение, пожалуйста, попробуйте ещё раз!", result);
        return RedirectToAction(nameof(EditProduct), new { productId });
    }
    public async Task<IActionResult> DeleteOrRestoreProduct(int productId, bool isToDelete)
    {
        var result = await _productService.DeleteOrRestoreProductAsync(productId, isToDelete);
        CheckResultAndCreateNotificationMessage(isToDelete ? "Продукт успешно удалён!" : "Продукт успешно восстановлен!",
            "К сожалению произошла ошибка! Обновите страницу и попробуйте снова.", result);
        return RedirectToAction(nameof(Index));
    }
    private async Task<List<CategoryDto>> GetCategories()
    {
        var categories = await _categoryService.GetCategoriesAsync(includeDeleted: true);
        if (categories?.Count < 1)
        {
            AddTempDataForNotification("Произошла неустановленная ошибка, пожалуйста, попробуйте ещё раз!", false);
            RedirectToAction(nameof(Index));
        }
        return categories;
    }
    private void CheckResultAndCreateNotificationMessage(string messageSuccess, string messageFail, bool result)
    {
        var notificationMessage = result ? messageSuccess : messageFail;
        AddTempDataForNotification(notificationMessage, result);
    }
    private void AddTempDataForNotification(string message, bool IsSuccessMessage)
    {
        TempData[ControllerExt.NotiNameForTempData] = message;
        if (!IsSuccessMessage) { TempData[ControllerExt.ErrorNameForTempData] = true; }
    }
}
