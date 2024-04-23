using GroceryStore.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.DTO;
public class CategoryDto
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Название - обязательное поле")]
    [MaxLength(50, ErrorMessage = "Длина названия не должна превышать {1} символов")]
    [Display(Name = "Название")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Описание - обязательное поле")]
    [MaxLength(500, ErrorMessage = "Длина описания не должна превышать {1} символов")]
    [Display(Name = "Описание")]
    public string Description { get; set; }
    [Range(0, int.MaxValue)]
    [Display(Name = "Количество продуктов")]
    public int ProductsCount { get; set; }
    [Required(ErrorMessage = "Статус - обязательное поле")]
    public bool IsDeleted { get; set; }
    public static Category ToCategory(CategoryDto dto)
    {
        return new Category
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            IsDeleted = dto.IsDeleted,
        };
    }
}
