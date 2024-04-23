using System.ComponentModel.DataAnnotations;

namespace GroceryStore.Core.DTO.Auth;
public class UserInfoModel
{
    [Required]
    [Display(Name = "Логин")]
    public string UserName { get; set; }
    [Required]
    [EmailAddress]
    [Display(Name = "Электронная почта")]
    public string Email { get; set; }
    [Required]
    [Display(Name = "Имя")]
    public string FirstName { get; set; }
    [Required]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; }
    [Required]
    [Display(Name = "Отчество")]
    public string MiddleName { get; set; }
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Дата рождения")]
    public DateTime BirthDate { get; set; }
}
