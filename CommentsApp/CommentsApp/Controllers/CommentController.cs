using CommentsApp.DTO.Comment;
using Microsoft.AspNetCore.Mvc;
using CommentsApp.Interfaces;

namespace CommentsApp.Controllers;


[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CommentController> _logger;

    public CommentController(ICommentService commentService, IHttpContextAccessor httpContextAccessor, ILogger<CommentController> logger)
    {
        _commentService = commentService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    // Отримання всіх коментарів
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetAllComments()
    {
        var user = _httpContextAccessor.HttpContext?.User.Identity?.Name;

        var comments = await _commentService.GetAllCommentsAsync();

        if (comments == null || !comments.Any())
        {
            _logger.LogInformation("No comments to return");
        }
        return Ok(comments);
    }

    // Створення нового коментаря
    [HttpPost]
    public async Task<ActionResult<CommentResponse>> CreateComment([FromBody] CreateCommentRequest request)
    {
        if (request == null)
            return BadRequest("Invalid data.");

        var comment = await _commentService.CreateCommentAsync(request);
        return CreatedAtAction(nameof(GetCommentById), new { id = comment.CommentId }, comment);
    }

    // Отримання конкретного коментаря за ID
    [HttpGet("{id}")]
    public async Task<ActionResult<CommentResponse>> GetCommentById(Guid id)
    {
        var comment = await _commentService.GetCommentByIdAsync(id);
        if (comment == null)
            return NotFound();

        return Ok(comment);
    }

    // Оновлення коментаря
    [HttpPut("{id}")]
    public async Task<ActionResult<CommentResponse>> UpdateComment(Guid id, [FromBody] UpdateCommentRequest request)
    {
        if (request == null || id != request.CommentId)
            return BadRequest("Invalid data.");

        var updatedComment = await _commentService.UpdateCommentAsync(request);
        if (updatedComment == null)
            return NotFound();

        return Ok(updatedComment);
    }

    // Видалення коментаря
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var result = await _commentService.DeleteCommentAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
