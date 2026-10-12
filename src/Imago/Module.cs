using Microsoft.Extensions.DependencyInjection;

namespace Imago;

/// <summary>
/// A part of an application that adds its services and sets them up when the application starts.
/// </summary>
/// <remarks>
/// When the application starts, every module registers its services first. Then every module boots, so a module can
/// use the services of other modules in <see cref="Boot"/>.
/// </remarks>
public abstract class Module
{
    /// <summary>
    /// Adds the services of this module.
    /// </summary>
    /// <param name="services">The collection to add the services to.</param>
    public virtual void Register(IServiceCollection services)
    {
        //
    }

    /// <summary>
    /// Sets up this module once the services of every module are available.
    /// </summary>
    /// <param name="app">The application that is starting.</param>
    public virtual void Boot(Application app)
    {
        //
    }
}
