using Microsoft.Data.Sqlite;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;

namespace Youmii.Infrastructure.Persistence;

/// <summary>
/// SQLite implementation of IFactRepository.
/// </summary>
public sealed class SqliteFactRepository : IFactRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SqliteFactRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task UpsertFactAsync(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Facts (Key, Value, UpdatedAt)
            VALUES (@key, @value, @updatedAt)
            ON CONFLICT(Key) DO UPDATE SET
                Value = excluded.Value,
                UpdatedAt = excluded.UpdatedAt;
            """;

        command.Parameters.AddWithValue("@key", key);
        command.Parameters.AddWithValue("@value", value);
        command.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Fact?> GetFactAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Key, Value, UpdatedAt
            FROM Facts
            WHERE Key = @key
            """;

        command.Parameters.AddWithValue("@key", key);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Fact
            {
                Key = reader.GetString(0),
                Value = reader.GetString(1),
                UpdatedAt = DateTime.Parse(reader.GetString(2))
            };
        }

        return null;
    }

    public async Task<IReadOnlyList<Fact>> GetAllFactsAsync()
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Key, Value, UpdatedAt
            FROM Facts
            ORDER BY Key
            """;

        var facts = new List<Fact>();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            facts.Add(new Fact
            {
                Key = reader.GetString(0),
                Value = reader.GetString(1),
                UpdatedAt = DateTime.Parse(reader.GetString(2))
            });
        }

        return facts;
    }
}
