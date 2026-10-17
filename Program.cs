using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/api/health", () =>
    Results.Ok(new
    {
        status = "Healthy",
        service = "MyApi",
        instance = Environment.MachineName
    }));

app.MapGet("/api/items", async (IConfiguration config) =>
{
    var connectionString =
        config.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return Results.Problem("Database connection string is missing.");
    }

    try
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var items = new List<object>();

        await using var command = new SqlCommand(
            "SELECT Id, Name FROM Items ORDER BY Id", connection);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new
            {
                id = reader.GetInt32(0),
                name = reader.GetString(1)
            });
        }

        return Results.Ok(items);
    }
    catch (SqlException)
    {
        return Results.Problem(
            "Could not retrieve items from SQL Server.",
            statusCode: 503);
    }
});

app.Run();