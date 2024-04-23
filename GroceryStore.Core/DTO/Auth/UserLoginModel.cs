using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.DTO.Auth;
public class UserLoginModel
{
    [Required]
    [Display(Name = "Логин")]
    public string UserNameOrEmail { get; set; }
    [DataType(DataType.Password)]
    [Required]
    [Display(Name = "Пароль")]
    public string Password { get; set; }
    [Required]
    [Display(Name = "Запомнить меня")]
    public bool IsPersistent { get; set; }
}
