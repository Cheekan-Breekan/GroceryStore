using GroceryStore.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ImagesController : ControllerBase
{
    private readonly IFileService _fileService;

    public ImagesController(IFileService fileService)
    {
        _fileService = fileService;
    }
    [HttpGet]
    [Route("GetImage")]
    [ResponseCache(Duration = 60 * 60 * 24, Location = ResponseCacheLocation.Any)]
    public IActionResult GetImage(string fileName)
    {
        var image = _fileService.GetImage(fileName);
        return File(image, "image/png");
    }
}
