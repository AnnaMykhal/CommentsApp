namespace CommentsApp.Configuration;

public class JwtSettings
{
    public string Key { get; set; }
    public int ExpiresInMinutes { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}
