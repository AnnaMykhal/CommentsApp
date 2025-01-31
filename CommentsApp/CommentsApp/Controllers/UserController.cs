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
    public async Task<IActionResult> Create(CreateUserRequest req)
    {
        if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Username) || string.IsNullOrEmpty(req.Password))
        {
            return BadRequest(new { message = "Email, Username, and Password are required" });
        }
        try
        {
            var res = await _userService.Create(req);
            return Ok(new { message = "User created successfully" });
        }
        catch (Exception e)
        {
            return BadRequest(new { message = $"An error occurred: {e.Message}" });
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

        if (avatar == null || avatar.Length == 0)
            return BadRequest(new { message = "Invalid avatar file" });

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profileImages");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileExtension = Path.GetExtension(avatar.FileName).ToLower();
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var relativePath = $"/profileImages/{uniqueFileName}";
        var absolutePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(absolutePath, FileMode.Create))
        {
            await avatar.CopyToAsync(stream);
        }

        var avatarPath = await _userService.UploadAvatar(user.Id, relativePath);

        return Ok(new { avatarPath });
    }
}
