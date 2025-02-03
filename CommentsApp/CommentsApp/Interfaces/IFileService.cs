namespace CommentsApp.Interfaces;

public interface IFileService
{
    Task<string> ResizeImageAsync(IFormFile file);
    Task<string> SaveTextFileAsync(IFormFile file);
}
