namespace Youmii.Core.DependencyInjection;

/// <summary>
/// Simple service locator interface for resolving dependencies.
/// Prefer constructor injection where possible.
/// </summary>
public interface IServiceLocator
{
    /// <summary>
    /// Resolves a service of the specified type.
    /// </summary>
    T Resolve<T>() where T : class;

    /// <summary>
    /// Resolves a service of the specified type, or null if not registered.
    /// </summary>
    T? ResolveOptional<T>() where T : class;
}
