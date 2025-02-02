using CommentsApp.Entities;

namespace CommentsApp.DTO.Users;

public class CreateUserResponse
{
    public Guid Id { get; set; } 
    public string Email { get; set; }
    public string Username { get; set; }
    public string? JwtToken { get; set; }
    public string AvatarUrl { get; set; }
    public string HomePage { get; set; }

    public CreateUserResponse(User user, string token, string avatarUrl = null)
    {
        Id = user.Id;
        Email = user.Email;
        Username = user.Name;
        JwtToken = token;
        AvatarUrl = AvatarUrl;
        HomePage = user.HomePage;
    }
}
