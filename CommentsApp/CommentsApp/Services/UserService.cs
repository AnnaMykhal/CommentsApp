using CommentsApp.Configuration;
using CommentsApp.Controllers;
using CommentsApp.Data;
using CommentsApp.DTO.Users;
using CommentsApp.Entities;
using CommentsApp.Interfaces;
using CommentsApp.Util;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CommentsApp.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly TokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserController> _logger;

    public UserService(AppDbContext context, IOptions<JwtSettings> jwtOptions, TokenService tokenService, IHttpContextAccessor httpContextAccessor, ILogger<UserController> logger)
    {
        _context = context;
        _jwtSettings = jwtOptions.Value;
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<CreateUserResponse> Create(CreateUserRequest req)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (existingUser != null)
        {
            _logger.LogInformation($"User with email '{req.Email}' already exists in the database.");
            return null;
        }

        if (!IsValidEmail(req.Email))
        {
            throw new ArgumentException("Invalid email format.");
        }
        var user = new User(req);
        user.PasswordHash = PasswordHash.Hash(req.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        if (user.Id != Guid.Empty)
        {
            string token = _tokenService.GenerateToken(_jwtSettings, user);
            return new CreateUserResponse( user, token);
        }
        throw new Exception("User creation failed.");
    }
    private bool IsValidEmail(string email)
    {
        try
        {
            var mailAddress = new System.Net.Mail.MailAddress(email);
            return mailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public async Task<AuthUserResponse> Authenticate(AuthUserRequest req)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == req.Username);
        if (user == null)
        {
            throw new Exception("Username or password incorrect");
        }

        var isPasswordValid = PasswordHash.Verify(req.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new Exception("Username or password incorrect");
        }

        string token = _tokenService.GenerateToken(_jwtSettings, user);
        _httpContextAccessor.HttpContext.Items["User"] = user;
        return new AuthUserResponse(user, token);
    }

    public async Task<IEnumerable<CreateUserResponse>> GetAll()
    {
        var users = await _context.Users.ToListAsync();

        var userResponses = users.Select(user =>
        {
            var avatarUrl = GenerateAvatarUrl(user.Id, user.AvatarUrl);
            return new CreateUserResponse(user, null, avatarUrl);
        });

        return userResponses;
    }


    public async Task<bool> Delete(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    private string GenerateAvatarUrl(Guid userId, string profileImage)
    {
        if (string.IsNullOrEmpty(profileImage))
            return null;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        var urlHelperFactory = httpContext.RequestServices.GetService<IUrlHelperFactory>();
        if (urlHelperFactory == null)
            return null;

        var actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        var urlHelper = urlHelperFactory.GetUrlHelper(actionContext);

        return urlHelper.Action("GetAvatar", "Users", new { userId }, httpContext.Request.Scheme);
    }

    public async Task<string> UploadAvatar(Guid userId, string avatarPath)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        user.AvatarUrl = avatarPath;
        await _context.SaveChangesAsync();

        return user.AvatarUrl;
    }

    public async Task<string> GetAvatarUrlAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.AvatarUrl))
        {
            return string.Empty;
        }

        var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Value}";
        var avatarUrl = $"{baseUrl}/{user.AvatarUrl}";

        return avatarUrl ?? string.Empty;
    }
}
