using GroceryStore.Core.DTO.Auth;

namespace GroceryStore.MVC.Models;

public class UserProfileVM
{
    public UserInfoModel UserInfoModel { get; set; }
    public UserChangePasswordModel UserChangePasswordModel { get; set; }
}
