using CommentsApp.Controllers;
using CommentsApp.Data;
using CommentsApp.DTO.Comment;
using CommentsApp.Entities;
using CommentsApp.Interfaces;
using CommentsApp.Services.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using RabbitMQ.Client;

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
        var (username, profileImage) = await GetUserDetailsAsync();

        if (string.IsNullOrEmpty(username))
        {
            username = "Anonymous";
            profileImage = "default-profile-image-path.jpg";
        }

        var (filePath, fileType) = await ProcessFileAsync(file);

        // Якщо користувач не авторизований, задаємо значення "Unknown"
        var userId = string.IsNullOrEmpty(username) ? null : (Guid?)await GetUserIdByUsernameAsync(username);

        // Створюємо новий коментар
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Text = request.Content,
            ParentCommentId = request.ParentCommentId,
            CreatedAt = DateTime.UtcNow,
            FilePath = filePath,
            FileExtension = fileType,
            Username = username ?? "Anonymous", 
            UserId = userId
        };

        // Якщо файл існує, додаємо шлях до черги
        if (!string.IsNullOrEmpty(filePath))
        {
            using var queue = new FileProcessingQueue(_connection, _channel);
            queue.Enqueue(filePath); // Додаємо завдання в чергу
        }

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return CreateCommentResponse(comment, profileImage, file);
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

        string? filePath = null;
        string? fileType = null;

        if (file.ContentType.StartsWith("image"))
        {
            filePath = await _fileService.ResizeImageAsync(file);
            fileType = "Image";
        }
        else if (file.ContentType == "text/plain")
        {
            filePath = await _fileService.SaveTextFileAsync(file);
            fileType = "Text";
        }
        else
        {
            throw new Exception("Unsupported file type.");
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
