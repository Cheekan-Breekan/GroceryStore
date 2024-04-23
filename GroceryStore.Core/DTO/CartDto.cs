using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.DTO;
public class CartDto
{
    public int Id { get; set; }
    [MaxLength(100)]
    public List<CartItemDto>? Items { get; set; } = new();
    public string UserId { get; set; }
    public bool IsOrdered { get; set; }
}
