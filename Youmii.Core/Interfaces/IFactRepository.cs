using Youmii.Core.Models;

namespace Youmii.Core.Interfaces;

/// <summary>
/// Repository for storing and retrieving user facts.
/// </summary>
public interface IFactRepository
{
    /// <summary>
    /// Inserts or updates a fact by key.
    /// </summary>
    Task UpsertFactAsync(string key, string value);

    /// <summary>
    /// Gets a fact by key, or null if not found.
    /// </summary>
    Task<Fact?> GetFactAsync(string key);

    /// <summary>
    /// Gets all stored facts.
    /// </summary>
    Task<IReadOnlyList<Fact>> GetAllFactsAsync();
}
