using Microsoft.Data.Sqlite;

namespace Youmii.Infrastructure.Persistence;

/// <summary>
/// Manages SQLite database initialization and connection.
/// </summary>
public sealed class DatabaseInitializer : IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;

    public DatabaseInitializer(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }

    /// <summary>
    /// Initializes the database schema if it doesn't exist.
    /// </summary>
    public async Task InitializeAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Messages (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Role TEXT NOT NULL,
                Content TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Facts (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS IX_Messages_CreatedAt ON Messages(CreatedAt DESC);
            """;

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Gets a new connection to the database.
    /// </summary>
    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
