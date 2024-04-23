using Microsoft.AspNetCore.Http;

namespace GroceryStore.Core.DTO;
public class ProductImagesDto
{
    public int Id { get; set; }
    public IFormFile? ImageMainFile { get; set; }
    public IFormFileCollection? ImageFiles { get; set; }
    public bool IsMain { get; set; } = false;
    public string? ImageMainPath { get; set; }
    public List<string>? ImagePaths { get; set; } = new();
}
