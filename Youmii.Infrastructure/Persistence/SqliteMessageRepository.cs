using Microsoft.Data.Sqlite;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;

namespace Youmii.Infrastructure.Persistence;

/// <summary>
/// SQLite implementation of IMessageRepository.
/// </summary>
public sealed class SqliteMessageRepository : IMessageRepository
{
    private readonly DatabaseInitializer _db;

    public SqliteMessageRepository(DatabaseInitializer db)
    {
        _db = db;
    }

    public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
    {
        await using var connection = _db.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Messages (Role, Content, CreatedAt)
            VALUES (@role, @content, @createdAt);
            SELECT last_insert_rowid();
            """;

        command.Parameters.AddWithValue("@role", message.Role);
        command.Parameters.AddWithValue("@content", message.Content);
        command.Parameters.AddWithValue("@createdAt", message.CreatedAt.ToString("O"));

        var id = (long)(await command.ExecuteScalarAsync())!;
        message.Id = id;

        return message;
    }

    public async Task<IReadOnlyList<ChatMessage>> GetRecentMessagesAsync(int limit)
    {
        await using var connection = _db.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Role, Content, CreatedAt
            FROM Messages
            ORDER BY CreatedAt DESC
            LIMIT @limit
            """;

        command.Parameters.AddWithValue("@limit", limit);

        var messages = new List<ChatMessage>();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            messages.Add(new ChatMessage
            {
                Id = reader.GetInt64(0),
                Role = reader.GetString(1),
                Content = reader.GetString(2),
                CreatedAt = DateTime.Parse(reader.GetString(3))
            });
        }

        // Return in chronological order (oldest first)
        messages.Reverse();
        return messages;
    }

    public async Task ClearAsync()
    {
        await using var connection = _db.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Messages;";
        await command.ExecuteNonQueryAsync();
    }
}
