using System.ComponentModel.DataAnnotations;

namespace CommentsApp.DTO.Comment;

public class UpdateCommentRequest
{
    [Required]
    public Guid CommentId { get; set; } 

    [Required]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Comment content must be between 1 and 500 characters.")]
    public string Text { get; set; } = null!;
}
