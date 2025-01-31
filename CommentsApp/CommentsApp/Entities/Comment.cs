using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;

namespace CommentsApp.Entities;


public class Comment 
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid? UserId { get; set; }
    public Guid? ParentCommentId { get; set; }

    [Required]
    [StringLength(1000)]
    public string Text { get; set; } = null!;

    [StringLength(100)]
    public string? Url { get; set; }

    [StringLength(10)]
    [RegularExpression(@"^\.(jpg|gif|png|txt)$", ErrorMessage = "Допустимі формати файлів: JPG, GIF, PNG, TXT.")]
    public string? FilePath { get; set; }

    [RegularExpression(@"^\.(jpg|png|gif|pdf|docx)$",
        ErrorMessage = "Допустимі розширення файлів: .jpg, .png, .gif, .pdf, .docx.")]
    [StringLength(10)]
    public string? FileExtension { get; set; }
    public long? FileSize { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string Username { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
  
    [ForeignKey("ParentCommentId")]
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
