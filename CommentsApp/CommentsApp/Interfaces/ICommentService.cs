using CommentsApp.DTO.Comment;

namespace CommentsApp.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<CommentResponse>> GetAllCommentsAsync();
    Task<CommentResponse> GetCommentByIdAsync(Guid id);
    Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request, IFormFile? file = null);
    Task<CommentResponse> UpdateCommentAsync(UpdateCommentRequest request, IFormFile? file = null);
    Task<bool> DeleteCommentAsync(Guid id);
}
