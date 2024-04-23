using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.Entities;
public class Product
{
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; }
    [Required]
    [Range(0, 999999)]
    public decimal Price { get; set; }
    [Required]
    [Range(0, 999999)]
    public int Quantity { get; set; }
    public Category Category { get; set; }
    [Required]
    [Range(0, int.MaxValue)]
    public int OrdersCount { get; set; }
    public string? ImageMainPath { get; set; }
    public List<string>? ImagePaths { get; set; } = new();
    [Required]
    public bool IsDeleted { get; set; }
}
