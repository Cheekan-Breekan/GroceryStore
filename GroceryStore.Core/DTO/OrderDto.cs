using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.DTO;
public class OrderDto
{
    public string Id { get; set; }
    [MinLength(1)]
    public List<CartItemDto> Items { get; set; } = new();
    [Required]
    public string UserId { get; set; }
    [Required]
    public decimal TotalPrice { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
}
