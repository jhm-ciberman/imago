using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Imago.Support;

/// <summary>
/// Provides extension methods for the <see cref="IServiceProvider"/> interface.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Gets the registered service of the given type, or creates a new object if it is not registered.
    /// </summary>
    /// <typeparam name="T">The type of the object. Its constructor can take services as parameters.</typeparam>
    /// <param name="services">The registered services, also used for the constructor.</param>
    /// <returns>The service or the new object.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the type is not registered and no constructor can be used with the services.</exception>
    public static T Make<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        this IServiceProvider services
    ) where T : class
    {
        return ActivatorUtilities.GetServiceOrCreateInstance<T>(services);
    }

    /// <summary>
    /// Creates a new object of the given type.
    /// </summary>
    /// <typeparam name="T">The type of the object. Its constructor can take services as parameters.</typeparam>
    /// <param name="services">The services for the constructor.</param>
    /// <param name="parameters">The constructor parameters that are not services.</param>
    /// <returns>The new object.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no constructor can be used with the given parameters and services.</exception>
    public static T Make<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        this IServiceProvider services,
        params object[] parameters
    ) where T : class
    {
        return ActivatorUtilities.CreateInstance<T>(services, parameters);
    }
}
