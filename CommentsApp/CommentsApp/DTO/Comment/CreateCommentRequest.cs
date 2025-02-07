using System.ComponentModel.DataAnnotations;

namespace CommentsApp.DTO.Comment;

public class CreateCommentRequest
{
    [Required]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Comment content must be between 1 and 500 characters.")]
    public string Content { get; set; } = null!;

    public Guid? ParentCommentId { get; set; }
    public string? UserName { get; set; }
}
