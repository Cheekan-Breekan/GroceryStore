namespace GroceryStore.Core.ResultModels;
public static class ErrorAuth
{
    public static readonly Error IncorrectData = new("Некорректные данные");
    public static readonly Error UserNotFound = new("Пользователь не найден");
    public static readonly Error IncorrectUsernameOrEmail = new("Неправильное имя/почта или пароль");
    public static readonly Error UnconfirmedEmail = new("Ваша почта не была подтверждена");
    public static readonly Error UserAlreadyExists = new("Пользователь с такими данными уже существует");
}
