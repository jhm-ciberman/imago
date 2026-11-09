using System;
using Microsoft.Extensions.DependencyInjection;

namespace Imago.Support;

/// <summary>
/// Gives access to the services of the application from anywhere.
/// </summary>
public sealed class Ioc : IServiceProvider
{
    /// <summary>
    /// Gets the instance that holds the services of the application.
    /// </summary>
    public static Ioc Default { get; } = new();

    private IServiceProvider? _services;

    private IServiceProvider Services => this._services
        ?? throw new InvalidOperationException("The services are not configured.");

    private Ioc()
    {
        //
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType)
    {
        return this.Services.GetService(serviceType);
    }

    /// <summary>
    /// Gets the service of the given type.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <returns>The service, or null if it is not registered.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the services are not configured.</exception>
    public T? GetService<T>() where T : class
    {
        return this.Services.GetService<T>();
    }

    /// <summary>
    /// Gets the service of the given type.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <returns>The service.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the services are not configured or the service is not registered.</exception>
    public T GetRequiredService<T>() where T : class
    {
        return this.Services.GetRequiredService<T>();
    }

    /// <summary>
    /// Sets the services of the application.
    /// </summary>
    /// <param name="serviceProvider">The services of the application.</param>
    /// <exception cref="InvalidOperationException">Thrown if the services are already configured.</exception>
    public void ConfigureServices(IServiceProvider serviceProvider)
    {
        if (this._services != null)
        {
            throw new InvalidOperationException("The services are already configured.");
        }

        this._services = serviceProvider;
    }
}
