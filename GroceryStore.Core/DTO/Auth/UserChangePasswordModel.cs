using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.DTO.Auth;
public class UserChangePasswordModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Текущий пароль")]
    public string OldPassword { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль")]
    public string NewPassword { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword))]
    [Display(Name = "Подтвердите новый пароль")]
    public string ConfirmNewPassword { get; set; }
    [DataType(DataType.EmailAddress)]
    [Required]
    [Display(Name = "Электронная почта")]
    public string Email { get; set; }
}
