using CommentsApp.Configuration;
using CommentsApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CommentsApp.Data;

namespace CommentsApp.Services;

public class TokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly AppDbContext _context;
    public TokenService(AppDbContext context, IOptions<JwtSettings> jwtOptions)
    {
        _context = context;
        _jwtSettings = jwtOptions.Value;
    }
    public string GenerateToken(JwtSettings jwtSettings, User user)
    {
        if (string.IsNullOrWhiteSpace(jwtSettings.Key))
            throw new ArgumentException("Secret key is missing in JWT settings.");

        if (jwtSettings.ExpiresInMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(jwtSettings.ExpiresInMinutes), "ExpirationInDays must be greater than zero.");

        if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
            throw new ArgumentException("Issuer is missing in JWT settings.");

        if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
            throw new ArgumentException("Audience is missing in JWT settings.");

        var key = Encoding.ASCII.GetBytes(jwtSettings.Key);

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
    };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
            Expires = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiresInMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
    
    
}
