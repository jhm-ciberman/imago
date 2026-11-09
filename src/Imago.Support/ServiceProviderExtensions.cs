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
