namespace GroceryStore.MVC.Models;

public class NotificationVM(string message, bool isError = false)
{
    public string Message { get; set; } = message;
    public bool IsError { get; set; } = isError;
}
