using GroceryStore.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace GroceryStore.Infrastructure;
public class FileService : IFileService
{
    private const string ImagesFolder = "Images";
    private static string[] AccessibleExtensions => ["jpg", "png", "jpeg", "bmp", "jfif", "gif"];
    public string UploadImage(IFormFile file)
    {
        if (file is null)
        {
            throw new ArgumentNullException("File is null!");
        }
        var extension = file.FileName.Split(".").Last();
        if (!AccessibleExtensions.Any(x => x == extension))
        {
            throw new ArgumentException("Extension not allowed!");
        }
        var savedFileName = Guid.NewGuid().ToString() + "." + extension;
        Directory.CreateDirectory(GetStoragePath());
        var savedFilePath = Path.Combine(GetStoragePath(), savedFileName);

        using (var stream = new FileStream(savedFilePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        return savedFileName;
    }
    public byte[] GetImage(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return [];
        }
        if (!File.Exists(Path.Combine(GetStoragePath(), fileName)))
        {
            return [];
        }
        return File.ReadAllBytes(Path.Combine(GetStoragePath(), fileName));
    }
    public void DeleteImage(string fileName)
    {
        var path = Path.Combine(GetStoragePath(), fileName);
        if (!File.Exists(path))
        {
            throw new ArgumentException($"File {fileName} not found!");
        }
        File.Delete(path);
    }
    private static string GetStoragePath() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ImagesFolder);
}
