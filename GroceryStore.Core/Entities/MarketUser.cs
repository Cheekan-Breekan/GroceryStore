using Microsoft.AspNetCore.Identity;

namespace GroceryStore.Core.Entities;
public class MarketUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public DateOnly BirthDate { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
