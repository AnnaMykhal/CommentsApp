using System.ComponentModel.DataAnnotations;

namespace CommentsApp.DTO.Users;

public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL.")]
    public string? HomePage { get; set; }

    public string? AvatarUrl { get; set; }
    public IFormFile? AvatarFile { get; set; }
}