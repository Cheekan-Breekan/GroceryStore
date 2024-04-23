using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.DTO.Auth;
public class UserEmailModel
{
    [EmailAddress]
    [Required]
    public string Email { get; set; }
}
