using CommentsApp.Controllers;
using CommentsApp.Data;
using CommentsApp.DTO.Comment;
using CommentsApp.Entities;
using CommentsApp.Interfaces;
using CommentsApp.Services.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using RabbitMQ.Client;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CommentsApp.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _context;
    private readonly IFileService _fileService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<CommentController> _logger;

    public CommentService(AppDbContext context, IFileService fileService, IHttpContextAccessor httpContextAccessor, IConnection connection, IModel channel, ILogger<CommentController> logger)
    {
        _context = context;
        _fileService = fileService;
        _httpContextAccessor = httpContextAccessor;
        _connection = connection;
        _channel = channel;
        _logger = logger;
    }

    public async Task<IEnumerable<CommentResponse>> GetAllCommentsAsync()
    {
        //var comments = await _context.Comments
        //    .Where(c => !c.IsDeleted)
        //    .Include(c => c.User)
        //    .Include(c => c.Replies)
        //    .ToListAsync();

        var comments = await _context.Comments
              .Where(c => !c.IsDeleted)
              .Include(c => c.User)
              .DefaultIfEmpty()
              .Include(c => c.Replies)
              .ToListAsync();

        if (comments == null || !comments.Any())
        {
            _logger.LogInformation("No comments found.");
        }

        var response = comments.Select(c => new CommentResponse(c)).ToList();
        return response;
    }

    public async Task<CommentResponse> GetCommentByIdAsync(Guid id)
    {
        var comment = await _context.Comments
            .Where(c => c.Id == id && !c.IsDeleted)
            .Include(c => c.User)
            .Include(c => c.Replies)
            .FirstOrDefaultAsync();

        if (comment == null)
            return null;

        return new CommentResponse(comment);
    }


    public async Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request, IFormFile? file = null)
    {
        Console.WriteLine($"Received comment: {request.Content}");
        Console.WriteLine($"ParentCommentId: {request.ParentCommentId}");

        // **Перевіряємо, чи користувач авторизований**
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usernameClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

        Guid? userId = null;
        string username = request.UserName ?? "Anonymous"; // Дозволяємо ввести ім'я вручну для гостей
        string profileImage = "/images/default-profile-image.jpg";

        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
            username = usernameClaim ?? "Unknown";

            var user = await _context.Users.FindAsync(userId);
            if (user != null && !string.IsNullOrEmpty(user.AvatarUrl))
            {
                profileImage = user.AvatarUrl;
            }
        }

        string relativeFilePath = string.Empty;
        string fileType = string.Empty;
        long? fileSize = null;

        var (UserName, AvatarUrl) = await GetUserDetailsAsync();
        if (string.IsNullOrEmpty(username))
        {
            username = "Unknown";
            profileImage = "/images/default-profile-image.jpg";
        }

        if (file != null)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"{Guid.NewGuid()}{fileExtension}";

            // Визначаємо тип файлу та папку
            var subFolder = IsDocumentFile(fileExtension) ? "documents" :
                            IsImageFile(fileExtension) ? "images" :
                            "others";

            fileType = IsDocumentFile(fileExtension) ? "document" :
                       IsImageFile(fileExtension) ? "image" : "other";

            Console.WriteLine($"File received: {file.FileName}, Extension: {fileExtension}, Type: {fileType}");

            // **Коректний шлях до папки `wwwroot/uploads/{subFolder}`**
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Абсолютний шлях до файлу
            var absoluteFilePath = Path.Combine(uploadsFolder, fileName);
            relativeFilePath = $"/uploads/{subFolder}/{fileName}"; // Шлях для збереження в БД
            fileSize = file.Length;

            Console.WriteLine($"Saving file to: {absoluteFilePath}");

            try
            {
                using (var stream = new FileStream(absoluteFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                Console.WriteLine("✅ File successfully saved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving file: {ex.Message}");
                throw;
            }
        }
        else
        {
            Console.WriteLine("⚠️ No file received.");
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Text = request.Content,
            ParentCommentId = request.ParentCommentId,
            CreatedAt = DateTime.UtcNow,
            FilePath = relativeFilePath,
            FileExtension = fileType,
            FileSize = fileSize,
            Username = username,
            UserId = userId // Якщо null – значить анонімний
        };


        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        Console.WriteLine("📝 Comment saved to database.");

        return CreateCommentResponse(comment, profileImage, file);
    }


    // Метод для перевірки, чи це документ
    private bool IsDocumentFile(string extension)
    {
        var documentExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".xls", ".xlsx", ".ppt", ".pptx" };
        return documentExtensions.Contains(extension);
    }

    // Метод для перевірки, чи це зображення
    private bool IsImageFile(string extension)
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" };
        return imageExtensions.Contains(extension);
    }

    public async Task<CommentResponse?> UpdateCommentAsync(UpdateCommentRequest request, IFormFile? file = null)
    {
        // Отримуємо коментар з бази
        var comment = await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == request.CommentId);

        if (comment == null) return null;

        // Отримуємо дані користувача
        var (username, profileImage) = await GetUserDetailsAsync();

        // Оновлюємо текст коментаря
        comment.Text = request.Text;
        comment.UpdatedAt = DateTime.UtcNow;

        // Обробляємо файл, якщо він є
        var (filePath, fileType) = await ProcessFileAsync(file);
        if (filePath != null)
        {
            comment.FilePath = filePath;
            comment.FileExtension = fileType;

            // Якщо файл існує, додаємо шлях до черги
            using var queue = new FileProcessingQueue(_connection, _channel);
            queue.Enqueue(filePath); // Додаємо завдання в чергу
        }

        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();

        // Формуємо відповідь
        return CreateCommentResponse(comment, profileImage, file);
    }

    public async Task<bool> DeleteCommentAsync(Guid id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null) return false;

        comment.IsDeleted = true;
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();

        return true;
    }


    private async Task<(string username, string? profileImage)> GetUserDetailsAsync()
    {
        string username = "Anonymous";
        string? profileImage = null;

        if (_httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true)
        {
            username = _httpContextAccessor.HttpContext.User.Identity.Name;

            var user = await _context.Users
                .Where(u => u.Name == username)
                .Select(u => new { u.AvatarUrl })
                .FirstOrDefaultAsync();

            if (user != null)
            {
                profileImage = user.AvatarUrl;
            }
        }

        return (username, profileImage);
    }

    private async Task<(string? filePath, string? fileType)> ProcessFileAsync(IFormFile? file)
    {
        if (file == null) return (null, null);

        string folderPath;
        string filePath;
        string fileType;

        if (file.ContentType.StartsWith("image"))
        {
            folderPath = Path.Combine("uploads", "images");
            Directory.CreateDirectory(folderPath); // Створюємо папку, якщо її немає

            filePath = await _fileService.ResizeImageAsync(file);
            fileType = "Image";
        }
        else if (file.ContentType == "text/plain")
        {
            folderPath = Path.Combine("uploads", "documents");
            Directory.CreateDirectory(folderPath);

            filePath = await _fileService.SaveTextFileAsync(file);
            fileType = "Text";
        }
        else
        {
            // Можна просто повернути null, щоб не викликати виняток
            return (null, null);
        }

        return (filePath, fileType);
    }

    private CommentResponse CreateCommentResponse(Comment comment, string? profileImage, IFormFile? file = null)
    {
        return new CommentResponse
        {
            CommentId = comment.Id,
            Content = comment.Text,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            ParentCommentId = comment.ParentCommentId,
            Username = comment.Username,
            ProfileImage = profileImage,
            FileUrl = comment.FilePath,
            FileSize = file?.Length,
            Replies = comment.Replies?
                .Select(reply => new CommentResponse(reply))
                .ToList() ?? new List<CommentResponse>()
        };
    }

    private async Task<Guid?> GetUserIdByUsernameAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == username);
        return user?.Id;  // Return null if user is not found
    }
}
