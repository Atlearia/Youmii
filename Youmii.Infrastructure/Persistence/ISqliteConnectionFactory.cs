using Youmii.Core.Interfaces;

namespace Youmii.Infrastructure.Persistence;

/// <summary>
/// SQLite database initializer implementing the core interface.
/// </summary>
public interface ISqliteConnectionFactory
{
    /// <summary>
    /// Creates a new connection to the database.
    /// </summary>
    Microsoft.Data.Sqlite.SqliteConnection CreateConnection();
}
