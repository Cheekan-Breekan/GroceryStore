namespace GroceryStore.MVC.Models;

public class Pager
{
    public int ItemsPerPage { get; set; }
    public int TotalPagesCount { get; set; }
    public int StartPage { get; set; }
    public int EndPage { get; set; }
    public int CurrentPage { get; set; }
    public int TotalItemsCount { get; set; }
    public string ControllerName { get; set; }
    public string ActionName { get; set; }
    public string CurrentSortOrder { get; set; }
    public string CurrentSearchFilter { get; set; }
    public int? CurrentSelectedCategory { get; set; }
    public Pager() { }
    public Pager(int totalItemsCount, int page, string controller, string action, int itemsPerPage = 10)
    {
        TotalPagesCount = (int)Math.Ceiling((decimal)totalItemsCount / itemsPerPage);
        CurrentPage = page;

        StartPage = CurrentPage - 5;
        EndPage = CurrentPage + 4;

        if (StartPage <= 0)
        {
            EndPage = EndPage - (StartPage - 1);
            StartPage = 1;
        }
        if (EndPage > TotalPagesCount)
        {
            EndPage = TotalPagesCount;
            if (EndPage > 10)
            {
                StartPage = EndPage - 9;
            }
        }
        TotalItemsCount = totalItemsCount;
        ItemsPerPage = itemsPerPage;
        ControllerName = controller;
        ActionName = action;
    }
}
