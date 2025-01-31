//using static System.Net.Mime.MediaTypeNames;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using CommentsApp.Interfaces;

namespace CommentsApp.Services;

public class FileService: IFileService
{
    private readonly string _fileStoragePath;

    public FileService(IConfiguration configuration)
    {
        var basePath = Directory.GetCurrentDirectory(); // Поточний робочий каталог
        _fileStoragePath = Path.Combine(basePath, "uploads"); // Створюємо шлях до папки
    }

    public async Task<string> ResizeImageAsync(IFormFile image)
    {
        using var imageStream = image.OpenReadStream();
        using var originalImage = Image.Load(imageStream);

        originalImage.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(320, 240)
        }));

        //var filePath = Path.Combine(_fileStoragePath, Guid.NewGuid() + Path.GetExtension(image.FileName));
        //await originalImage.SaveAsync(filePath);

        //return filePath;
        var filePath = Path.Combine(_fileStoragePath, Guid.NewGuid().ToString() + Path.GetExtension(image.FileName));

        // Зберігання файлу
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        return filePath;
    }

    public async Task<string> SaveTextFileAsync(IFormFile textFile)
    {
        if (textFile.Length > 102_400) // 100 КБ
        {
            throw new Exception("Text file exceeds the size limit of 100 KB.");
        }

        //var filePath = Path.Combine(_fileStoragePath, Guid.NewGuid() + ".txt");
        //using var stream = new FileStream(filePath, FileMode.Create);
        //await textFile.CopyToAsync(stream);

        //return filePath;
        var filePath = Path.Combine(_fileStoragePath, Guid.NewGuid().ToString() + Path.GetExtension(textFile.FileName));

        // Зберігання файлу
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await textFile.CopyToAsync(stream);
        }

        return filePath;
    }
}
