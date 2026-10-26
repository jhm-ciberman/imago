using Imago.Diagnostics;
using Imago.Scenarios;
using Microsoft.Extensions.DependencyInjection;

namespace Imago;

/// <summary>
/// A part of an application that adds its services and sets them up when the application is created.
/// </summary>
/// <remarks>
/// When the application is created, every module registers its services first. Then every module boots, so a module can
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
    /// Changes the logging settings, such as adding a sink. Called before the application is created.
    /// </summary>
    /// <param name="logging">The logging settings.</param>
    public virtual void ConfigureLogging(LoggingBuilder logging)
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

    /// <summary>
    /// Adds the tools of this module to a scenario. Called once, when a scenario starts.
    /// </summary>
    /// <param name="scenario">The scenario that is starting.</param>
    public virtual void BootScenario(Scenario scenario)
    {
        //
    }
}
