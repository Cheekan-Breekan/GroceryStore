namespace GroceryStore.Core.ResultModels;
public class Error
{
    public string Title { get; }
    public Dictionary<string, string> Details { get; } = [];
    public static readonly Error None = new(string.Empty);
    public Error(string title, Dictionary<string, string> details = null)
    {
        if (details is not null)
        {
            Details = details;
        }
        Title = title;
    }
}
