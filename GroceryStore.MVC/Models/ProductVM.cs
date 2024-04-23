using GroceryStore.Core.DTO;

namespace GroceryStore.MVC.Models;

public class ProductVM
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public CategoryDto? Category { get; set; }
    public int CategoryId { get; set; }
    public int OrdersCount { get; set; }
    public bool IsInCart { get; set; }
    public string? ImageMainPath { get; set; }
    public static ProductVM FromDto(ProductDto product)
    {
        return new ProductVM
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            Category = product.Category,
            CategoryId = product.Category.Id,
            OrdersCount = product.OrdersCount,
            ImageMainPath = product.ImageMainPath,
        };
    }
}
