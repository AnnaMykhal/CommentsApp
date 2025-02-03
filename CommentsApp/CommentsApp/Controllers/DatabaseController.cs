using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CommentsApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly string _connectionString = "Server=localhost;Database=YourDatabaseName;User Id=sa;Password=YourPassword;";

    [HttpGet("test-connection")]
    public IActionResult TestDatabaseConnection()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            return Ok("Database connection successful!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Database connection failed: {ex.Message}");
        }
    }
}
