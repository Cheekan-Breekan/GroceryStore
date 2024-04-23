using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.DTO.Auth;
public class UserResetPasswordModel
{
    [Required]
    public string Id { get; set; }
    [Required]
    public string Token { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль")]
    public string NewPassword { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword))]
    [Display(Name = "Подтвердите новый пароль")]
    public string ConfirmNewPassword { get; set; }
}
