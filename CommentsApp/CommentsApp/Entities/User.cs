using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace CommentsApp.Entities;

public class User : IdentityUser<Guid>
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
