using Microsoft.Data.Sqlite;
using Youmii.Core.Interfaces;

namespace Youmii.Infrastructure.Persistence;

/// <summary>
/// Manages SQLite database initialization and connection.
/// </summary>
public sealed class SqliteDatabaseInitializer : IDatabaseInitializer, ISqliteConnectionFactory, IDisposable
{
    private readonly string _connectionString;
    private bool _disposed;

    public SqliteDatabaseInitializer(string dbPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dbPath);
        _connectionString = $"Data Source={dbPath}";
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        ThrowIfDisposed();

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

    /// <inheritdoc />
    public SqliteConnection CreateConnection()
    {
        ThrowIfDisposed();
        return new SqliteConnection(_connectionString);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        _disposed = true;
    }
}
