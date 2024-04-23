using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.Entities;
public class Cart
{
    public int Id { get; set; }
    [MaxLength(100)]
    public List<CartItem>? Items { get; set; } = new();
    public string UserId { get; set; }
    public bool IsOrdered { get; set; }
}
