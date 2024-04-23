using GroceryStore.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.MVC.Controllers;
public class ImagesController : Controller
{
    private readonly IFileService _fileService;
    public ImagesController(IFileService fileService)
    {
        _fileService = fileService;
    }
    [ResponseCache(Duration = (60 * 60 * 24), Location = ResponseCacheLocation.Any)]
    public IActionResult GetImage(string imagePath)
    {
        var image = _fileService.GetImage(imagePath);
        return File(image, "image/png");
    }
}
