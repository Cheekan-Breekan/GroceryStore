using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.DTO.Auth;
public class UserRegisterModel : UserInfoModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Подтверждение пароля")]
    public string ConfirmPassword { get; set; }
}
