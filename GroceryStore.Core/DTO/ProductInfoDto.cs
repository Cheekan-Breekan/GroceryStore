using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.DTO;
public class ProductInfoDto
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
    public static ProductDto ToProductDto(ProductInfoDto product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            CategoryId = product.CategoryId
        };
    }
    public static ProductInfoDto FromProductDto(ProductDto product)
    {
        return new ProductInfoDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            CategoryId = product.CategoryId
        };
    }
}
