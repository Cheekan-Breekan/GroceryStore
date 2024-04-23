using Microsoft.AspNetCore.Http;

namespace GroceryStore.Core.Interfaces;
public interface IFileService
{
    string UploadImage(IFormFile file);
    byte[] GetImage(string fileName);
    void DeleteImage(string fileName);
}
