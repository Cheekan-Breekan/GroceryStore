using GroceryStore.Core.Entities;
using GroceryStore.Core.Extensions;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.DTO;
public class ProductDto
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Наименование - обязательное поле")]
    [MaxLength(50, ErrorMessage = "Длина наименования не должна превышать 50 символов")]
    [Display(Name = "Наименование")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Описание - обязательное поле")]
    [MaxLength(1000, ErrorMessage = "Длина описания не должна превышать 1000 символов")]
    [Display(Name = "Описание")]
    public string Description { get; set; }
    [Required(ErrorMessage = "Цена - обязательное поле")]
    [Range(0, 999999, ErrorMessage = "Цена допустима в диапазоне от 0 до 999999")]
    [Display(Name = "Цена")]
    public decimal Price { get; set; }
    [Required(ErrorMessage = "Количество - обязательное поле")]
    [Range(0, 999999, ErrorMessage = "Количество на складе допустимо в диапазоне от 0 до 999999")]
    [Display(Name = "Количество на складе")]
    public int Quantity { get; set; }
    [Required(ErrorMessage = "Необходимо выбрать категорию")]
    [Display(Name = "Категория")]
    public int CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
    [Required(ErrorMessage = "Количество заказов - обязательное поле")]
    [Range(0, int.MaxValue, ErrorMessage = "Количество заказов допустимо в диапазоне от 0 до 2147483647")]
    [Display(Name = "Количество заказов")]
    public int OrdersCount { get; set; }
    public string? ImageMainPath { get; set; }
    public List<string>? ImagePaths { get; set; } = new();
    //
    [Display(Name = "Лицевое изображение")]
    public IFormFile? ImageMain { get; set; }
    [Display(Name = "Дополнительные изображения")]
    [MaxFileCount(10)]
    public IFormFileCollection? Images { get; set; }
    [Required(ErrorMessage = "Статус - обязательное поле")]
    public bool IsDeleted { get; set; }

    public static Product ToProduct(ProductDto product)
    {
        return new Product
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            Category = CategoryDto.ToCategory(product.Category),
            OrdersCount = product.OrdersCount,
            ImageMainPath = product.ImageMainPath,
            ImagePaths = product.ImagePaths
        };
    }
}
