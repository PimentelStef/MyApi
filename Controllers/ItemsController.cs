using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ItemsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetItems()
    {
        var connectionString =
            _configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return StatusCode(500, "Database connection is not configured.");
        }

        var items = new List<Item>();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT Id, Name
            FROM dbo.Items
            ORDER BY Id;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new Item(
                reader.GetInt32(0),
                reader.GetString(1)));
        }

        return Ok(items);
    }

    public record Item(int Id, string Name);
}