using CommentsApp.DTO.Users;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommentsApp.Util;


namespace CommentsApp.Entities;

public class User : IdentityUser<Guid>
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = null!;
    public bool IsAdmin { get; set; } = false;
    //public bool IsEmailConfirmed { get; set; } = false; 
    public string? AvatarUrl { get; set; }
    [Url(ErrorMessage = "Please enter a valid URL.")]
    public string? HomePage { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public User(CreateUserRequest req)
    {
        Email = req.Email;
        Name = req.Username;
        PasswordHash = CommentsApp.Util.PasswordHash.Hash(req.Password);
        HomePage = req.HomePage;
        AvatarUrl = req.AvatarUrl;
    }
   
    public User() { }
}
