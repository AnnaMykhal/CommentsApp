using CommentsApp.DTO.Users;
using CommentsApp.Entities;
using CommentsApp.Interfaces;
using CommentsApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommentsApp.Data;
using Newtonsoft.Json;


namespace CommentsApp.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;
    private readonly TokenService _tokenService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, AppDbContext context, TokenService tokenService, ILogger<UserController> logger)
    {
        _userService = userService;
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest req)
    {
        Console.WriteLine($"User controller req: {JsonConvert.SerializeObject(req)}");
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid input data", details = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }
        try
        {// Перевірка, чи є файл
            if (req.AvatarFile == null || req.AvatarFile.Length == 0)
            {
                return BadRequest(new { message = "Avatar file is required" });
            }

            // Передаємо дані в сервіс для створення користувача та обробки файлу
            var res = await _userService.Create(req);
            return Ok(new { message = "User created successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while creating user");

            return StatusCode(500, new { message = "An internal server error occurred.", details = e.Message });
        }
    }

    [HttpPost("authenticate")]
    [AllowAnonymous]
    public async Task<IActionResult> Authenticate(AuthUserRequest req)
    {
        try
        {
            var response = await _userService.Authenticate(req);

            if (response == null)
            {
                ModelState.AddModelError("", "Username or password incorrect");
                return BadRequest(new { message = "Username or password incorrect" });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Email not confirmed"))
            {
                return BadRequest(new { message = ex.Message });
            }

            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CreateUserResponse>>> GetAllUsers()
    {
        var users = await _userService.GetAll();
        return Ok(users);
    }

    //[Authorize]
    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        var user = HttpContext.Items["User"] as User;

        if (user == null)
            return BadRequest(new { message = "User not found" });

        await _userService.Delete(user.Id);
        return Ok();
    }


    [HttpPost("upload-avatar")]
    public async Task<IActionResult> UploadAvatar([FromForm] IFormFile avatar)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Unauthorized(new { message = "User not authorized" });
        }

        if (!int.TryParse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value, out int userId))
        {
            return Unauthorized(new { message = "User ID not found in claims" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BadRequest(new { message = "User not found" });

        if (avatar == null || avatar.Length == 0)
            return BadRequest(new { message = "Invalid avatar file" });

        // Викликаємо метод у UserService для завантаження аватара
        var avatarUrl = await _userService.UploadAvatar(user.Id, avatar);

        return Ok(new { avatarUrl });
    }

    [HttpGet("avatars/{userId}")]
    public async Task<IActionResult> GetAvatar(Guid userId)
    {
        // Отримуємо шлях до аватарки через UserService
        var avatarPath = await _userService.GetAvatarUrlAsync(userId);

        // Якщо аватарка не знайдена, повертаємо 404
        if (string.IsNullOrEmpty(avatarPath))
        {
            return NotFound("Avatar not found.");
        }

        // Читання файлу аватарки
        var fileBytes = await System.IO.File.ReadAllBytesAsync(avatarPath);

        // Повертаємо аватарку як зображення
        return File(fileBytes, "image/jpeg");
    }
}
