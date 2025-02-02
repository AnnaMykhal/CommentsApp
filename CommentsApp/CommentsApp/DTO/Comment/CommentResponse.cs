using CommentsApp.Entities;

namespace CommentsApp.DTO.Comment;

public class CommentResponse
{
    public Guid CommentId { get; set; } 
    public string Content { get; set; } = null!; 
    public DateTime CreatedAt { get; set; } 
    public DateTime? UpdatedAt { get; set; } 
    public Guid? ParentCommentId { get; set; } 
    public string Username { get; set; } = null!;
    public string? Email { get; set; }
    public string? ProfileImage { get; set; }
    public string? FileUrl { get; set; } 
    public long? FileSize { get; set; }
    public List<CommentResponse> Replies { get; set; } = new();

    public string CreatedAtFormatted { get; set; }
    public string UpdatedAtFormatted { get; set; }
    public CommentResponse(CommentsApp.Entities.Comment comment)
    {
        CommentId = comment.Id;
        Content = comment.Text;
        CreatedAtFormatted = comment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
        UpdatedAtFormatted = comment.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss");
        ParentCommentId = comment.ParentCommentId;
        Username = comment.User?.UserName ?? "Anonymous";
        Email = comment.User?.Email ?? "no-email@example.com";
        ProfileImage = comment.User?.AvatarUrl;
        Replies = comment.Replies
            ?.Select(reply => new CommentResponse(reply))
            .ToList() ?? new List<CommentResponse>();
    }

    public CommentResponse()
    {
    }
}
