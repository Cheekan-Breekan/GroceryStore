using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.Entities;
public class Category
{
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    [Required]
    [MaxLength(500)]
    public string Description { get; set; }
    [Range(0, int.MaxValue)]
    public List<Product> Products { get; set; } = new();
    [Required]
    public bool IsDeleted { get; set; }
}
