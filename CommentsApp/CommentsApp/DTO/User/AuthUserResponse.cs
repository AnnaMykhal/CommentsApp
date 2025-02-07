using CommentsApp.DTO.Comment;
using CommentsApp.Entities;

namespace CommentsApp.DTO.Users;

public class AuthUserResponse
{
    public Guid Id { get; set; } 
    public string Username { get; set; }
    public string Email { get; set; }
    public string JwtToken { get; set; }
    public string AvatarUrl { get; set; }
    public string HomePage { get; set; }
    public List<CommentResponse> Comments { get; set; }

    public AuthUserResponse(User user, string token)
    {
        Id = user.Id;
        Username = user.Name; 
        Email = user.Email;
        JwtToken = token;
        AvatarUrl = user.AvatarUrl;
        HomePage = user.HomePage;
        Comments = user.Comments.Select(comment => new CommentResponse
        {
            CommentId = comment.Id,
            Content = comment.Text,
            CreatedAt = comment.CreatedAt
        }).ToList();
    }
}

    