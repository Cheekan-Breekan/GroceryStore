using GroceryStore.Core.DTO;
using GroceryStore.Core.Interfaces.Services;
using GroceryStore.MVC.Extensions;
using GroceryStore.MVC.Filters;
using GroceryStore.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.MVC.Controllers;
[Authorize(Roles = "Admin")]
public class CategoriesEditorController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly int tablePageSize = 20;
    public CategoriesEditorController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }


    [DisplayNotification]
    public async Task<IActionResult> Index(string sortOrder, string searchFilter, string currentFilter, int page = 1)
    {
        ViewBag.CurrentSortOrder = sortOrder;
        ViewBag.NameSortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "name_desc" : string.Empty;
        ViewBag.IsDeletedSortOrder = sortOrder == "deleted" ? "deleted_desc" : "deleted";
        ViewBag.ProductsCount = sortOrder == "products" ? "products_desc" : "products";

        if (!string.IsNullOrWhiteSpace(searchFilter))
        {
            page = 1;
        }
        else
        {
            searchFilter = currentFilter;
        }
        ViewBag.CurrentSearchFilter = searchFilter;

        var categoriesCount = await _categoryService.CountCategoriesAsync(searchFilter, includeDeleted: true);
        page = page < 1 ? 1 : page;
        var pager = new Pager(categoriesCount, page, ControllerExt.NameOf<CategoriesEditorController>(), nameof(Index), tablePageSize);

        ViewBag.Pager = pager;

        var categories = await _categoryService.GetCategoriesByFilterSortPagingAsync(searchFilter, sortOrder, page, tablePageSize, includeDeleted: true);
        return View(categories);
    }
    public IActionResult AddCategory()
    {
        return View(nameof(EditCategory), new CategoryDto());
    }
    [HttpPost]
    public async Task<IActionResult> AddCategory(CategoryDto category)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Notification = new NotificationVM("Не удалось! Заполните поля правильно.", true);
            return View(nameof(EditCategory), category);
        }
        if (await _categoryService.CategoryExistsByNameAsync(category.Name))
        {
            ModelState.AddModelError("NotUnique", "Категория с данным наименованием уже существует!");
            ViewBag.Notification = new NotificationVM("Данное наименование для категории уже существует!", true);
            return View(nameof(EditCategory), category);
        }

        var result = await _categoryService.AddCategoryAsync(category);
        if (!result)
        {
            ViewBag.Notification = new NotificationVM("Не удалось создать категорию", true);
            return View(nameof(EditCategory), new CategoryDto());
        }

        AddTempDataForNotification($"Категория {category.Name} успешно создана!", true);
        return RedirectToAction(nameof(Index));
    }

    [DisplayNotification]
    public async Task<IActionResult> EditCategory(int categoryId)
    {
        var category = await _categoryService.GetCategoryAsync(categoryId);
        if (category is null)
        {
            AddTempDataForNotification("Данная категория не была найдена, пожалуйста, попробуйте ещё раз!", false);
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }
    [HttpPost]
    public async Task<IActionResult> EditCategory(CategoryDto category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }
        var result = await _categoryService.UpdateCategoryAsync(category);
        if (!result)
        {
            AddTempDataForNotification("Не удалось обновить данные, пожалуйста, попробуйте ещё раз!", false);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Notification = new NotificationVM($"Категория {category.Name} успешно отредактирована!");
        return View(category);
    }
    public async Task<IActionResult> DeleteOrRestoreCategory(int categoryId, bool isToDelete)
    {
        var category = await _categoryService.GetCategoryAsync(categoryId);
        if (category?.ProductsCount > 0)
        {
            AddTempDataForNotification($"Невозможно удалить данную категорию, т.к. к ней приписаны продаваемые продукты. Их количество: {category.ProductsCount}", false);
            return RedirectToAction(nameof(EditCategory), new { categoryId });
        }

        var result = await _categoryService.DeleteOrRestoreCategoryAsync(categoryId, isToDelete);
        if (!result)
        {
            AddTempDataForNotification("Не удалось удалить категорию, пожалуйста, попробуйте ещё раз!", true);
            return RedirectToAction(nameof(Index));
        }

        var message = isToDelete ? $"Категория успешно удалена!" : $"Категория успешно восстановлена!";
        AddTempDataForNotification(message, true);
        return RedirectToAction(nameof(Index));
    }

    private void AddTempDataForNotification(string message, bool IsSuccessMessage)
    {
        TempData[ControllerExt.NotiNameForTempData] = message;
        if (!IsSuccessMessage) { TempData[ControllerExt.ErrorNameForTempData] = true; }
    }
}
