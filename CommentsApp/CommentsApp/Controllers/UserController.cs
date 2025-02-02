using CommentsApp.DTO.Users;
using CommentsApp.Entities;
using CommentsApp.Interfaces;
using CommentsApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommentsApp.Data;

namespace CommentsApp.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;
    private readonly TokenService _tokenService;

    public UserController(IUserService userService, AppDbContext context, TokenService tokenService)
    {
        _userService = userService;
        _context = context;
        _tokenService = tokenService;
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateUserRequest req, [FromForm] IFormFile avatar)
    {
        if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Username) || string.IsNullOrEmpty(req.Password))
        {
            return BadRequest(new { message = "Email, Username, and Password are required" });
        }

        try
        {
            var avatarUrl = await HandleAvatarUpload(avatar);
            Console.WriteLine($"Uploading avatar for user {req.Username}. File: {avatar.FileName}");
            req.AvatarUrl = avatarUrl; 

            var res = await _userService.Create(req);
            return Ok(new { message = "User created successfully" });
        }
        catch (Exception e)
        {
            return BadRequest(new { message = $"An error occurred: {e.Message}" });
        }
    }
    //[HttpPost]
    //public async Task<IActionResult> Create(CreateUserRequest req)
    //{
    //    if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Username) || string.IsNullOrEmpty(req.Password))
    //    {
    //        return BadRequest(new { message = "Email, Username, and Password are required" });
    //    }
    //    try
    //    {
    //        var res = await _userService.Create(req);
    //        return Ok(new { message = "User created successfully" });
    //    }
    //    catch (Exception e)
    //    {
    //        return BadRequest(new { message = $"An error occurred: {e.Message}" });
    //    }
    //}

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

    //[Authorize]
    //[HttpPost("upload-avatar")]
    //public async Task<IActionResult> UploadAvatar([FromForm] IFormFile avatar)
    //{
    //    if (!User.Identity.IsAuthenticated)
    //    {
    //        return Unauthorized(new { message = "User not authorized" });
    //    }

    //    if (!int.TryParse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value, out int userId))
    //    {
    //        return Unauthorized(new { message = "User ID not found in claims" });
    //    }

    //    var user = await _context.Users.FindAsync(userId);
    //    if (user == null)
    //        return BadRequest(new { message = "User not found" });

    //    if (avatar == null || avatar.Length == 0)
    //        return BadRequest(new { message = "Invalid avatar file" });

    //    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profileImages");
    //    if (!Directory.Exists(uploadsFolder))
    //        Directory.CreateDirectory(uploadsFolder);

    //    var fileExtension = Path.GetExtension(avatar.FileName).ToLower();
    //    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
    //    var relativePath = $"/profileImages/{uniqueFileName}";
    //    var absolutePath = Path.Combine(uploadsFolder, uniqueFileName);

    //    using (var stream = new FileStream(absolutePath, FileMode.Create))
    //    {
    //        await avatar.CopyToAsync(stream);
    //    }

    //    var avatarPath = await _userService.UploadAvatar(user.Id, relativePath);

    //    return Ok(new { avatarPath });
    //}
    [Authorize]
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

        var avatarUrl = await HandleAvatarUpload(avatar);  
        user.AvatarUrl = avatarUrl;

        await _context.SaveChangesAsync();
        return Ok(new { avatarUrl });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null)
        {
            return Unauthorized(new { message = "User ID not found in claims" });
        }

        if (!int.TryParse(userIdClaim.Value, out int userId))
        {
            return BadRequest(new { message = "Invalid user ID format" });
        }

        var user = _context.Users.Find(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(new
        {
            id = user.Id,
            username = user.Name,
            email = user.Email,
            avatar = user.AvatarUrl
        });
    }

    private async Task<string> HandleAvatarUpload(IFormFile avatar)
    {
        if (avatar == null || avatar.Length == 0)
            return null;

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profileImages");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);
        Console.WriteLine($"HandleAvatarUpload  uploadsFolder  {uploadsFolder}");
        var fileExtension = Path.GetExtension(avatar.FileName).ToLower();
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var relativePath = $"/profileImages/{uniqueFileName}";
        var absolutePath = Path.Combine(uploadsFolder, uniqueFileName);
        Console.WriteLine($"HandleAvatarUpload  absolutePath  {absolutePath}");
        using (var stream = new FileStream(absolutePath, FileMode.Create))
        {
            await avatar.CopyToAsync(stream);
        }

        return relativePath;
    }
}
