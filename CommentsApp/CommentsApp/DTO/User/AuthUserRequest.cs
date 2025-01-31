using System.ComponentModel.DataAnnotations;

namespace CommentsApp.DTO.Users
{
    public class AuthUserRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
