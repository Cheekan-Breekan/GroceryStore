using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.Entities;
public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [MinLength(1)]
    public List<CartItem> Items { get; set; }
    [Required]
    public string UserId { get; set; }
    [Required]
    public int CartId { get; set; }
    [Required]
    public decimal TotalPrice { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
}
