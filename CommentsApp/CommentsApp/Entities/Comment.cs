using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;

namespace CommentsApp.Entities;


public class Comment 
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public Guid? ParentCommentId { get; set; }

    [Required]
    [StringLength(1000)]
    public string Text { get; set; } = null!;

    [StringLength(100)]
    public string? Url { get; set; }

    [RegularExpression(@"^([a-zA-Z]:\\|\\\\)([^\\/:*?""<>|]+\\)*[^\\/:*?""<>|]+(\.[a-zA-Z0-9]+)?$",
    ErrorMessage = "Неправильний формат шляху до файлу.")]
    [StringLength(255)]
    public string? FilePath { get; set; }

    [StringLength(10)]
    public string? FileExtension { get; set; }
   
    public DateTime CreatedAt { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; }
}
