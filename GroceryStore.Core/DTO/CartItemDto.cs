using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.DTO;
public class CartItemDto
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public ProductDto? Product { get; set; }
    [Range(0, 100)]
    public int Quantity { get; set; }
}
