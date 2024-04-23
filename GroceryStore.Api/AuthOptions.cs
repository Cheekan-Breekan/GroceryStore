using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GroceryStore.Api;

public static class AuthOptions
{
    public const string Issuer = "GroceryStoreServer";
    public const string Audience = "GroceryStoreClient";
    private const string Key = "5q1YCeAx5uBUP4oDdEDYp5EGg8TijtkS0Fm4eIVOy73dGHWZIttiIIuMN9UGCfEzpr4_BBOHaiyRzFGAv_Cmyu9jyUo2_D_8EAhDDpMYykCX5GeCsJxM_ysrF_qdWR1nyBtC8FbQTOVHQFBeuc4_ZV601HIkC5QfHAVRUEDtLFM";
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
}
